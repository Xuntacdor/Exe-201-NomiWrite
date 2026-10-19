using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NomiWrite.Learning.Application.DTOs;
using NomiWrite.Learning.Application.Interfaces;
using NomiWrite.Learning.Domain.Entities;
using NomiWrite.Learning.Infrastructure.Options;

namespace NomiWrite.Learning.Infrastructure.Services;

public class GroqQuizProvider : IAiQuizProvider
{
    private const string FixedCategoryList =
        "Mạo từ; Hòa hợp chủ ngữ - động từ; Chia thì động từ; Số ít / số nhiều của danh từ; " +
        "Giới từ; Câu điều kiện; Mệnh đề quan hệ; Câu bị động; Trật tự từ trong câu; " +
        "Liên từ và từ nối; Đại từ; So sánh hơn / so sánh nhất; Động từ khuyết thiếu; " +
        "Danh động từ và động từ nguyên mẫu; Cấu trúc câu; Dùng từ sai loại; " +
        "Lặp từ / diễn đạt dài dòng; Dấu câu; Khác";

    private const string AllowedTypes = "multiple_choice, fill_blank, rewrite";

    private const int MaxRateLimitRetriesPerModel = 2;
    private const int BaseBackoffSeconds = 2;
    private const int MaxBackoffSeconds = 16;

    private readonly HttpClient _httpClient;
    private readonly IOptions<GroqSettings> _groqOptions;
    private readonly ILogger<GroqQuizProvider> _logger;

    public GroqQuizProvider(
        IHttpClientFactory httpClientFactory,
        IOptions<GroqSettings> groqOptions,
        ILogger<GroqQuizProvider> logger)
    {
        _httpClient = httpClientFactory.CreateClient("Groq");
        _groqOptions = groqOptions;
        _logger = logger;

        if (!string.IsNullOrWhiteSpace(_groqOptions.Value.ApiKey) &&
            !_httpClient.DefaultRequestHeaders.Contains("Authorization"))
        {
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_groqOptions.Value.ApiKey}");
        }
    }

    public async Task<List<QuizQuestionItem>> GenerateQuestionsAsync(
        QuizGenerationRequest request,
        int targetCount)
    {
        var prompt = BuildPrompt(request, targetCount);
        var endpoint = BuildEndpoint();
        var models = ResolveModels();

        var failures = new List<string>();

        foreach (var model in models)
        {
            var body = new
            {
                model,
                messages = new[]
                {
                    new { role = "user", content = prompt }
                },
                temperature = 0.7,
                max_tokens = 2048
            };

            for (var attempt = 1; attempt <= MaxRateLimitRetriesPerModel; attempt++)
            {
                using var response = await _httpClient.PostAsJsonAsync(endpoint, body);

                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        return await ParseQuestionsAsync(response);
                    }
                    catch (Exception exception) when (exception is InvalidOperationException or JsonException)
                    {
                        _logger.LogWarning(
                            exception,
                            "Groq model {Model} returned an unusable quiz response; switching to the next model.",
                            model);
                        failures.Add($"{model}: {exception.Message}");
                        break;
                    }
                }

                var errorPayload = await response.Content.ReadAsStringAsync();

                if (IsTransientStatus(response.StatusCode) && attempt < MaxRateLimitRetriesPerModel)
                {
                    _logger.LogWarning(
                        "Groq model {Model} returned transient {StatusCode} (attempt {Attempt}/{MaxAttempts}); retrying.",
                        model,
                        (int)response.StatusCode,
                        attempt,
                        MaxRateLimitRetriesPerModel);
                    await DelayBackoffAsync(GetBackoff(attempt), CancellationToken.None);
                    continue;
                }

                _logger.LogWarning(
                    "Groq model {Model} unavailable ({StatusCode}): {Error}. Switching to the next model.",
                    model,
                    (int)response.StatusCode,
                    errorPayload);
                failures.Add($"{model}: HTTP {(int)response.StatusCode}");
                break;
            }
        }

        _logger.LogError("All Groq models failed for quiz generation. Attempts: {Failures}", string.Join(" | ", failures));
        throw new HttpRequestException("Quiz generation is temporarily unavailable (no AI model available).");
    }

    // Rate limits usually clear within seconds, so back off exponentially
    // (2s, 4s, ...) with jitter before retrying the same model.
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
    // (404 unknown model, 401 auth, 400 bad request) switches immediately.
    private static bool IsTransientStatus(HttpStatusCode statusCode) =>
        statusCode == HttpStatusCode.TooManyRequests || (int)statusCode >= 500;

    private string BuildEndpoint()
    {
        var options = _groqOptions.Value;
        return string.IsNullOrWhiteSpace(options.Endpoint)
            ? "https://api.groq.com/openai/v1/chat/completions"
            : options.Endpoint;
    }

    private List<string> ResolveModels()
    {
        var configured = (_groqOptions.Value.Models ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(model => !string.IsNullOrWhiteSpace(model))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (configured.Count == 0)
            configured.Add("openai/gpt-oss-120b");

        return configured;
    }

    private async Task<List<QuizQuestionItem>> ParseQuestionsAsync(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        var text = ExtractText(content);

        if (string.IsNullOrWhiteSpace(text))
            throw new InvalidOperationException("Groq returned an empty quiz response.");

        var questions = DeserializePayload(text);

        if (questions?.Count == 0)
            throw new InvalidOperationException("Groq returned no quiz questions.");

        return questions ?? new List<QuizQuestionItem>();
    }

    private string BuildPrompt(QuizGenerationRequest request, int targetCount)
    {
        var grammarErrors = request.GrammarErrors?.Take(10) ?? new List<GrammarErrorDto>();
        var vocab = request.Vocabulary?.Take(10) ?? new List<VocabDto>();

        var grammarSnapshot = JsonSerializer.Serialize(grammarErrors.Select(g => new
        {
            category = g.GrammarCategory,
            sentence = g.Sentence,
            suggestion = g.Suggestion,
            explanation = g.Explanation
        }), new JsonSerializerOptions { WriteIndented = true });

        var vocabSnapshot = JsonSerializer.Serialize(vocab.Select(v => new
        {
            originalWord = v.OriginalWord,
            suggestedWord = v.SuggestedWord,
            exampleSentence = v.ExampleSentence
        }), new JsonSerializerOptions { WriteIndented = true });

        var builder = new StringBuilder();

        builder.AppendLine("You are a TOEFL-IELTS English teacher helping the learner strengthen weak points.");
        builder.AppendLine("Generate a short practice quiz as STRICT JSON with no markdown fences.");
        builder.AppendLine($"Aim for {targetCount} questions.");
        builder.AppendLine($"Allowed question types are: {AllowedTypes}.");
        builder.AppendLine($"Fixed grammar category values (only these may be used for grammar questions, otherwise \"Khác\"): {FixedCategoryList}.");
        builder.AppendLine("Vocabulary questions must use category \"Vocabulary\".");
        builder.AppendLine();

        if (!string.IsNullOrWhiteSpace(request.Hint))
            builder.AppendLine($"Focus hint: {request.Hint}");

        builder.AppendLine();
        builder.AppendLine("Grammar mistakes detected in the learner's writing:");
        builder.AppendLine(grammarSnapshot);
        builder.AppendLine();
        builder.AppendLine("Vocabulary suggestions for the learner:");
        builder.AppendLine(vocabSnapshot);
        builder.AppendLine();

        builder.AppendLine("Rules:");
        builder.AppendLine("- Use the learner's real sentences/words from the data above; never invent unrelated topics.");
        builder.AppendLine("- For grammar: design a question around the exact mistake (correct it, fix word order, fill the blank, pick the best option).");
        builder.AppendLine("- For vocabulary: reuse the suggested word as the expected answer.");
        builder.AppendLine("- multiple_choice: provide 4 options in \"options\"; correctAnswer must equal exactly one option.");
        builder.AppendLine("- fill_blank: put the blank as ____ in \"sentence\"; correctAnswer is the expected word/phrase.");
        builder.AppendLine("- rewrite: correctAnswer is a reference corrected sentence.");
        builder.AppendLine("- Keep \"id\" short and unique (e.g. q1).");

        builder.AppendLine();
        builder.AppendLine("Respond with exactly this shape:");
        builder.AppendLine("{ \"questions\": [ { \"id\": \"q1\", \"category\": \"...\", \"type\": \"...\", \"question\": \"...\", \"sentence\": \"...\", \"options\": [ \"...\" ], \"correctAnswer\": \"...\", \"explanation\": \"...\" } ] }");

        return builder.ToString();
    }

    private static string ExtractText(string content)
    {
        using var document = JsonDocument.Parse(content);
        var root = document.RootElement;

        if (root.TryGetProperty("error", out var error) &&
            error.TryGetProperty("message", out var errorMessage))
        {
            throw new InvalidOperationException($"Groq error: {errorMessage.GetString()}");
        }

        if (!root.TryGetProperty("choices", out var choices))
            throw new InvalidOperationException("Groq response missing 'choices'.");

        var textBuilder = new StringBuilder();

        foreach (var choice in choices.EnumerateArray())
        {
            if (choice.TryGetProperty("message", out var message) &&
                message.TryGetProperty("content", out var contentProperty))
            {
                textBuilder.AppendLine(contentProperty.GetString());
            }
        }

        return textBuilder.ToString().Trim();
    }

    private static List<QuizQuestionItem>? DeserializePayload(string text)
    {
        var json = text.Trim();

        if (json.StartsWith("```", StringComparison.Ordinal))
        {
            var start = json.IndexOf('{');
            var end = json.LastIndexOf('}');
            if (start >= 0 && end > start)
                json = json[start..(end + 1)];
        }

        var payload = JsonSerializer.Deserialize<QuizPayload>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return payload?.questions;
    }

    private sealed class QuizPayload
    {
        public List<QuizQuestionItem>? questions { get; set; }
    }
}
