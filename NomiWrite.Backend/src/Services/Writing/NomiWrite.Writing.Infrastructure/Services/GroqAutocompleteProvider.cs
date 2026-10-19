using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NomiWrite.Writing.Application.Interfaces;
using NomiWrite.Writing.Infrastructure.Options;

namespace NomiWrite.Writing.Infrastructure.Services;

/// <summary>
/// Groq-backed inline autocomplete. Tries each configured model in order and
/// returns the first usable continuation. Any failure (HTTP error, parse error,
/// empty content) is swallowed and treated as "no suggestion" so a flaky model
/// never breaks the writer's editor.
/// </summary>
public class GroqAutocompleteProvider : IAutocompleteProvider
{
    private const int MaxTokens = 40;
    private const int MaxRateLimitRetriesPerModel = 2;
    private const int BaseBackoffSeconds = 1;
    private const int MaxBackoffSeconds = 4;

    private const string SystemPrompt =
        "You are an autocomplete engine for an English essay. Continue the writer's text naturally " +
        "in the SAME language, tone and style. Output ONLY the continuation of 10 to 25 words. " +
        "Do not repeat any existing text. Do not add quotes, labels or explanations. " +
        "Stop at a natural sentence boundary.";

    private readonly HttpClient _httpClient;
    private readonly IOptions<GroqSettings> _groqOptions;
    private readonly ILogger<GroqAutocompleteProvider> _logger;

    public GroqAutocompleteProvider(
        IHttpClientFactory httpClientFactory,
        IOptions<GroqSettings> groqOptions,
        ILogger<GroqAutocompleteProvider> logger)
    {
        _httpClient = httpClientFactory.CreateClient("GroqAutocomplete");
        _groqOptions = groqOptions;
        _logger = logger;

        if (!string.IsNullOrWhiteSpace(_groqOptions.Value.ApiKey) &&
            !_httpClient.DefaultRequestHeaders.Contains("Authorization"))
        {
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_groqOptions.Value.ApiKey}");
        }
    }

    public async Task<string?> CompleteAsync(AutocompleteContext context, CancellationToken cancellationToken)
    {
        var prefix = (context?.Prefix ?? string.Empty).Trim();
        if (prefix.Length == 0)
            return null;

        var endpoint = BuildEndpoint();
        var models = ResolveModels();
        var userContent = BuildUserContent(context!.WritingPrompt, prefix);
        var failures = new List<string>();

        foreach (var model in models)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var body = new
            {
                model,
                messages = new[]
                {
                    new { role = "system", content = SystemPrompt },
                    new { role = "user", content = userContent }
                },
                temperature = 0.2,
                top_p = 0.9,
                max_tokens = MaxTokens,
                stop = new[] { "\n\n" }
            };

            for (var attempt = 1; attempt <= MaxRateLimitRetriesPerModel; attempt++)
            {
                try
                {
                    using var response = await _httpClient.PostAsJsonAsync(endpoint, body, cancellationToken);

                    if (response.IsSuccessStatusCode)
                    {
                        var suggestion = await ParseSuggestionAsync(response, cancellationToken);
                        if (!string.IsNullOrEmpty(suggestion))
                            return suggestion;

                        _logger.LogWarning("Groq autocomplete model {Model} returned an empty suggestion.", model);
                        failures.Add($"{model}: empty suggestion");
                        break;
                    }

                    var errorPayload = await response.Content.ReadAsStringAsync(cancellationToken);

                    if (IsTransientStatus(response.StatusCode) && attempt < MaxRateLimitRetriesPerModel)
                    {
                        _logger.LogWarning(
                            "Groq autocomplete model {Model} returned transient {StatusCode} (attempt {Attempt}/{Max}); retrying.",
                            model,
                            (int)response.StatusCode,
                            attempt,
                            MaxRateLimitRetriesPerModel);
                        await DelayBackoffAsync(GetBackoff(attempt), cancellationToken);
                        continue;
                    }

                    _logger.LogWarning(
                        "Groq autocomplete model {Model} unavailable ({StatusCode}): {Error}. Trying next model.",
                        model,
                        (int)response.StatusCode,
                        errorPayload);
                    failures.Add($"{model}: HTTP {(int)response.StatusCode}");
                    break;
                }
                catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
                {
                    // HttpClient timeout (not the caller aborting): let the next
                    // model try instead of failing the request.
                    _logger.LogWarning("Groq autocomplete model {Model} timed out; trying next model.", model);
                    failures.Add($"{model}: timeout");
                    break;
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception exception) when (exception is HttpRequestException or JsonException or NotSupportedException)
                {
                    _logger.LogWarning(exception, "Groq autocomplete model {Model} call failed; trying next model.", model);
                    failures.Add($"{model}: {exception.Message}");
                    break;
                }
            }
        }

        _logger.LogWarning("Autocomplete produced no suggestion. Attempts: {Failures}", string.Join(" | ", failures));
        return null;
    }

    private static async Task<string?> ParseSuggestionAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        var payload = await response.Content.ReadFromJsonAsync<GroqChatResponse>(cancellationToken: cancellationToken);
        var content = payload?.Choices is { Count: > 0 } ? payload.Choices[0].Message?.Content : null;
        return Sanitize(content);
    }

    private static string? Sanitize(string? content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return null;

        var text = content.Replace("\r", " ").Replace("\n", " ").Trim();

        if (text.Length >= 2 &&
            ((text.StartsWith('"') && text.EndsWith('"')) ||
             (text.StartsWith('\u201c') && text.EndsWith('\u201d'))))
        {
            text = text[1..^1].Trim();
        }

        while (text.Contains("  ", StringComparison.Ordinal))
            text = text.Replace("  ", " ", StringComparison.Ordinal);

        return text.Length == 0 ? null : text;
    }

    private static TimeSpan GetBackoff(int attempt)
    {
        var exponentialSeconds = Math.Min(MaxBackoffSeconds, BaseBackoffSeconds * (1 << (attempt - 1)));
        var jitterMs = Random.Shared.Next(0, 251);
        return TimeSpan.FromSeconds(exponentialSeconds) + TimeSpan.FromMilliseconds(jitterMs);
    }

    // Virtual so unit tests can skip the real wait while still exercising the retry loop.
    protected virtual Task DelayBackoffAsync(TimeSpan delay, CancellationToken cancellationToken) =>
        Task.Delay(delay, cancellationToken);

    private static bool IsTransientStatus(HttpStatusCode statusCode) =>
        statusCode == HttpStatusCode.TooManyRequests || (int)statusCode >= 500;

    private string BuildEndpoint()
    {
        var endpoint = _groqOptions.Value.Endpoint;
        return string.IsNullOrWhiteSpace(endpoint)
            ? "https://api.groq.com/openai/v1/chat/completions"
            : endpoint;
    }

    private static string BuildUserContent(string writingPrompt, string prefix)
    {
        var builder = new StringBuilder();

        if (!string.IsNullOrWhiteSpace(writingPrompt))
        {
            builder.Append("Essay prompt: ").Append(writingPrompt.Trim()).AppendLine();
            builder.AppendLine();
        }

        builder.AppendLine("Essay so far:");
        builder.Append(prefix);
        return builder.ToString();
    }

    private List<string> ResolveModels()
    {
        var configured = (_groqOptions.Value.Models ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(model => !string.IsNullOrWhiteSpace(model))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (configured.Count == 0)
            configured.Add("qwen/qwen3.8-27b");

        return configured;
    }

    private sealed class GroqChatResponse
    {
        public List<GroqChoice>? Choices { get; set; }
    }

    private sealed class GroqChoice
    {
        public GroqMessage? Message { get; set; }
    }

    private sealed class GroqMessage
    {
        public string? Content { get; set; }
    }
}
