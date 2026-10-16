using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NomiWrite.AICoordinator.Application.DTOs;
using NomiWrite.AICoordinator.Application.Interfaces;
using NomiWrite.AICoordinator.Domain.Entities;
using NomiWrite.AICoordinator.Infrastructure.Options;

namespace NomiWrite.AICoordinator.Infrastructure.Services;

public class OpenRouterGradingProvider : IAiGradingProvider
{
    private readonly HttpClient _httpClient;
    private readonly OpenRouterSettings _settings;
    private readonly IGradingDbContext _dbContext;
    private readonly IAiSecretProtector _secretProtector;
    private readonly IMemoryCache _cache;
    private readonly ILogger<OpenRouterGradingProvider> _logger;

    private const string ActiveConfigCacheKey = "ai_grading_active_config";
    private static readonly TimeSpan ActiveConfigCacheDuration = TimeSpan.FromSeconds(60);

    // Per-model retry only for rate limiting; a different free model is tried
    // when a model is unavailable, paid-only, or returns unusable output.
    private const int MaxRateLimitRetriesPerModel = 2;
    private const int BaseBackoffSeconds = 2;
    private const int MaxBackoffSeconds = 16;

    private const string DefaultGradingPrompt = """
        You are an experienced IELTS Writing examiner. Grade the following IELTS Writing essay based on the official IELTS scoring criteria.

        Score the essay on a scale of 0-9 for each of the following four criteria:
        1. Task Achievement - How well the essay addresses the task requirements, presents a clear position, and develops relevant ideas.
        2. Coherence and Cohesion - How well the essay is organized, paragraphed, and uses cohesive devices to connect ideas.
        3. Lexical Resource - The range and accuracy of vocabulary used, including paraphrasing and word choice.
        4. Grammatical Range and Accuracy - The variety and accuracy of grammatical structures used.

        Also provide:
        - An overall band score (calculate as the average of the four criteria, rounded to the nearest 0.5).
        - A short overall feedback paragraph (3-5 sentences) summarizing the essay's strengths and areas for improvement.
        - Up to 20 inline grammar or spelling errors found in the text. Identify EVERY grammar, spelling, punctuation, or word-choice error in the text, no matter how minor. If the text has zero errors, return an empty array — but do not skip errors that are present. For each error, provide the exact original text, a suggested correction, and a brief one-line explanation.

        Be strict but fair in your grading. Use the standard IELTS band descriptors.

        Respond with STRICT JSON only (no markdown fences, no commentary) using exactly this shape:
        {
          "overallBand": 6.5,
          "criteria": [
            { "name": "Task Achievement", "score": 6.5, "comment": "..." }
          ],
          "overallFeedback": "...",
          "grammarErrors": [
            { "originalText": "...", "suggestion": "...", "explanation": "..." }
          ],
          "vocabularySuggestions": [
            { "originalWord": "...", "suggestedAlternatives": ["...", "..."], "context": "..." }
          ],
          "restructuringSuggestions": [
            { "originalSentence": "...", "suggestedRewrite": "...", "reason": "..." }
          ]
        }
        """;

    public OpenRouterGradingProvider(
        HttpClient httpClient,
        IOptions<OpenRouterSettings> settings,
        IGradingDbContext dbContext,
        IAiSecretProtector secretProtector,
        IMemoryCache cache,
        ILogger<OpenRouterGradingProvider> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _dbContext = dbContext;
        _secretProtector = secretProtector;
        _cache = cache;
        _logger = logger;
    }

    public async Task<AiGradingResponseSchema> GradeEssayAsync(string essayContent)
    {
        var config = await GetActiveConfigAsync();

        var models = ResolveModels(config);
        var apiKey = ResolveApiKey(config);
        var systemPrompt = !string.IsNullOrWhiteSpace(config?.SystemPromptTemplate)
            ? config!.SystemPromptTemplate!
            : DefaultGradingPrompt;
        var endpoint = string.IsNullOrWhiteSpace(_settings.Endpoint)
            ? "https://openrouter.ai/api/v1/chat/completions"
            : _settings.Endpoint;
        var temperature = config?.Temperature ?? 0.2m;
        var maxTokens = config?.MaxOutputTokens ?? 4096;

        var failures = new List<string>();

        foreach (var model in models)
        {
            var body = new
            {
                model,
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = essayContent }
                },
                temperature,
                max_tokens = maxTokens
            };

            for (var attempt = 1; attempt <= MaxRateLimitRetriesPerModel; attempt++)
            {
                using var response = await SendRequestAsync(endpoint, body, apiKey);

                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        return await ParseOpenRouterResponseAsync(response);
                    }
                    catch (Exception exception) when (exception is InvalidOperationException or JsonException)
                    {
                        _logger.LogWarning(
                            exception,
                            "OpenRouter model {Model} returned an unusable grading response; switching to the next free model.",
                            model);
                        failures.Add($"{model}: {exception.Message}");
                        break;
                    }
                }

                var errorBody = await response.Content.ReadAsStringAsync();

                if (IsTransientStatus(response.StatusCode) && attempt < MaxRateLimitRetriesPerModel)
                {
                    _logger.LogWarning(
                        "OpenRouter model {Model} returned transient {StatusCode} (attempt {Attempt}/{MaxAttempts}); retrying.",
                        model,
                        (int)response.StatusCode,
                        attempt,
                        MaxRateLimitRetriesPerModel);
                    await DelayBackoffAsync(GetBackoff(attempt), CancellationToken.None);
                    continue;
                }

                _logger.LogWarning(
                    "OpenRouter model {Model} unavailable ({StatusCode}): {ErrorBody}. Switching to the next free model.",
                    model,
                    (int)response.StatusCode,
                    errorBody);
                failures.Add($"{model}: HTTP {(int)response.StatusCode}");
                break;
            }
        }

        _logger.LogError("All OpenRouter free models failed. Attempts: {Failures}", string.Join(" | ", failures));
        throw new HttpRequestException(
            "AI grading is temporarily unavailable because no free AI model could complete the request. Please try again shortly.");
    }

    // Rate limits on free models usually clear within seconds, so back off
    // exponentially (2s, 4s, ...) with jitter before retrying the same model.
    private static TimeSpan GetBackoff(int attempt)
    {
        var exponentialSeconds = Math.Min(
            MaxBackoffSeconds,
            BaseBackoffSeconds * (1 << (attempt - 1)));
        var jitterMs = Random.Shared.Next(0, 501);
        return TimeSpan.FromSeconds(exponentialSeconds) + TimeSpan.FromMilliseconds(jitterMs);
    }

    // Virtual so unit tests can skip the real wait while still exercising the
    // retry loop.
    protected virtual Task DelayBackoffAsync(TimeSpan delay, CancellationToken cancellationToken) =>
        Task.Delay(delay, cancellationToken);

    // 429 and 5xx are worth a short retry on the same model; everything else
    // (402 payment required, 404 unknown model, 401 auth) switches immediately.
    private static bool IsTransientStatus(HttpStatusCode statusCode) =>
        statusCode == HttpStatusCode.TooManyRequests || (int)statusCode >= 500;

    private List<string> ResolveModels(AiGradingConfig? config)
    {
        var models = new List<string>();

        if (IsFreeModel(config?.ModelName))
            models.Add(config!.ModelName.Trim());

        if (IsFreeModel(config?.FallbackModelName))
            models.Add(config!.FallbackModelName!.Trim());

        models.AddRange(SplitModels(_settings.Models));
        models.AddRange(SplitModels(_settings.FallbackModels));

        var resolved = models
            .Where(m => !string.IsNullOrWhiteSpace(m))
            .Select(m => m.Trim())
            .Where(IsFreeModel)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (resolved.Count == 0)
            resolved.Add("openrouter/free");

        return resolved;
    }

    private static IEnumerable<string> SplitModels(string value) =>
        string.IsNullOrWhiteSpace(value)
            ? Array.Empty<string>()
            : value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    /// <summary>
    /// Only models that OpenRouter serves for free (a ":free" suffix, or the
    /// "openrouter/free" router) are ever called, so a paid subscription can
    /// never be triggered accidentally.
    /// </summary>
    private static bool IsFreeModel(string? model)
    {
        if (string.IsNullOrWhiteSpace(model))
            return false;

        var trimmed = model.Trim();
        return trimmed.EndsWith(":free", StringComparison.OrdinalIgnoreCase)
            || string.Equals(trimmed, "openrouter/free", StringComparison.OrdinalIgnoreCase);
    }

    private string ResolveApiKey(AiGradingConfig? config)
    {
        if (!string.IsNullOrWhiteSpace(config?.ApiKeyCiphertext))
            return _secretProtector.Unprotect(config.ApiKeyCiphertext);

        if (!string.IsNullOrWhiteSpace(_settings.ApiKey))
            return _settings.ApiKey;

        throw new InvalidOperationException("No OpenRouter API key is configured.");
    }

    private async Task<HttpResponseMessage> SendRequestAsync(string endpoint, object requestBody, string apiKey)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = JsonContent.Create(requestBody)
        };
        request.Headers.Add("Authorization", $"Bearer {apiKey}");
        request.Headers.Add("X-Title", "NomiWrite");
        return await _httpClient.SendAsync(request);
    }

    private static async Task<AiGradingResponseSchema> ParseOpenRouterResponseAsync(HttpResponseMessage response)
    {
        var payload = await response.Content.ReadFromJsonAsync<OpenRouterApiResponse>();

        if (payload?.Error is not null && !string.IsNullOrWhiteSpace(payload.Error.Message))
            throw new InvalidOperationException($"OpenRouter error: {payload.Error.Message}");

        var text = payload?.Choices?.FirstOrDefault()?.Message?.Content
            ?? throw new InvalidOperationException("OpenRouter response did not contain any text content.");

        var json = ExtractJsonPayload(text);
        var gradingResponse = JsonSerializer.Deserialize<AiGradingResponseSchema>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? throw new InvalidOperationException("Failed to deserialize OpenRouter grading response.");

        return gradingResponse;
    }

    private static string ExtractJsonPayload(string text)
    {
        var trimmed = text.Trim();
        if (trimmed.StartsWith("```", StringComparison.Ordinal))
        {
            var firstNewLine = trimmed.IndexOf('\n');
            var lastFence = trimmed.LastIndexOf("```", StringComparison.Ordinal);
            if (firstNewLine >= 0 && lastFence > firstNewLine)
                trimmed = trimmed[(firstNewLine + 1)..lastFence].Trim();
        }

        if (trimmed.StartsWith('{') && trimmed.EndsWith('}'))
            return trimmed;

        var start = trimmed.IndexOf('{');
        if (start < 0)
            return trimmed;

        var depth = 0;
        var inString = false;
        var escaped = false;
        var builder = new StringBuilder();

        for (var i = start; i < trimmed.Length; i++)
        {
            var character = trimmed[i];
            builder.Append(character);

            if (escaped)
            {
                escaped = false;
                continue;
            }

            if (character == '\\' && inString)
            {
                escaped = true;
                continue;
            }

            if (character == '"')
            {
                inString = !inString;
                continue;
            }

            if (inString)
                continue;

            if (character == '{')
                depth++;
            else if (character == '}')
            {
                depth--;
                if (depth == 0)
                    return builder.ToString();
            }
        }

        return trimmed;
    }

    /// <summary>
    /// Returns the active (IsActive=true) config, most recently updated.
    /// Cached for 60 s to avoid a DB round-trip on every grading call.
    /// </summary>
    private async Task<AiGradingConfig?> GetActiveConfigAsync()
    {
        if (_cache.TryGetValue(ActiveConfigCacheKey, out AiGradingConfig? cached))
            return cached;

        var config = await _dbContext.AiGradingConfigs
            .AsNoTracking()
            .Where(c => c.IsActive)
            .OrderByDescending(c => c.UpdatedAt)
            .FirstOrDefaultAsync();

        _cache.Set(ActiveConfigCacheKey, config, ActiveConfigCacheDuration);

        return config;
    }

    /// <summary>
    /// Called by AdminAiConfigService after an update so the provider picks up
    /// the new config immediately without waiting for the cache TTL.
    /// </summary>
    public void InvalidateActiveConfigCache()
    {
        _cache.Remove(ActiveConfigCacheKey);
    }

    private sealed class OpenRouterApiResponse
    {
        public List<OpenRouterChoice>? Choices { get; set; }
        public OpenRouterError? Error { get; set; }
    }

    private sealed class OpenRouterChoice
    {
        public OpenRouterMessage? Message { get; set; }
    }

    private sealed class OpenRouterMessage
    {
        public string? Content { get; set; }
    }

    private sealed class OpenRouterError
    {
        public string? Message { get; set; }
    }
}
