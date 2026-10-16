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

public class OpenRouterStudyGuideProvider : IStudyGuideAiProvider
{
    private const string AllowedFocusValues = "grammar, vocabulary, coherence, task_response, lexical";
    private const string AllowedActionTypes = "write_essay, review_history, practice_vocabulary, practice_quiz, none";

    private const int MaxRateLimitRetriesPerModel = 2;
    private const int BaseBackoffSeconds = 2;
    private const int MaxBackoffSeconds = 16;

    private const string SystemPrompt = """
        You are a senior IELTS / VSTEP writing examiner and a personalized learning-path coach.
        Your job is to analyze a learner's ENTIRE writing history and turn it into a concrete,
        prioritized improvement roadmap that closes the gap between their current level and their target.

        YOUR LEARNER IS A VIETNAMESE UNIVERSITY STUDENT. English is their second language, so every
        point you make must be immediately actionable in plain language, with a short Vietnamese
        explanation attached. Never assume they understand examiner jargon like "Grammatical Range
        and Accuracy" or "Lexical Resource targets".

        INPUT DATA (JSON) you will receive:
        - learnerProfile: the learner's saved current level, target exam/type, and target band (fields may be absent).
        - essaySummaries: the most recent graded essays. Each contains an overallBand, a createdAt date,
          and criteriaScores (e.g. Task Achievement, Coherence and Cohesion, Lexical Resource,
          Grammatical Range and Accuracy).
        - topics: the essay topics this learner has already written about.
        - grammarAggregates: recurring grammar error categories with occurrence counts and example sentences.
        - vocabulary: recently suggested word upgrades, each marked with whether the learner already mastered it.
        - quizStats: optional accuracy of the learner's recent practice quizzes.

        TASK:
        Diagnose the learner across the four official writing criteria. Compare essays chronologically to identify
        which criteria are improving, stable, or declining, then emit a STRICT JSON response
        (no markdown fences, no commentary outside the JSON) with EXACTLY this shape:
        {
          "summary": "2-3 sentence overall assessment, referencing real data (current band, strongest and weakest areas).",
          "estimatedBand": 6.5,
          "strengths": [
            { "text": "English statement naming the criterion explicitly", "explanationVi": "one-sentence Vietnamese explanation, plain language" }
          ],
          "weaknesses": [
            { "text": "English statement naming the criterion/category explicitly", "explanationVi": "one-sentence Vietnamese explanation, plain language" }
          ],
          "nextSteps": [
            {
              "title": "< 12 words",
              "description": "one concrete, doable action (< 30 words)",
              "explanationVi": "one short Vietnamese sentence telling them exactly what to do today",
              "focus": "grammar|vocabulary|coherence|task_response|lexical",
              "actionType": "write_essay|review_history|practice_vocabulary|practice_quiz|none",
              "actionTarget": "a relative route like /write?focus=grammar or /history or /vocabulary or /quiz"
            },
            { "... two more steps ..." }
          ],
          "recommendedTopic": {
            "title": "a specific next essay topic",
            "reason": "why this topic now (tie it to the learner's actual data)",
            "suggestedPrompt": "a full IELTS Writing Task 2 style prompt for that topic",
            "ideaHints": ["Point 1: short argument", "Point 2: short counterargument", "Point 3: short supporting idea"],
            "keyVocabulary": ["collocation or topic term", "another term", "another term"]
          }
        }

        RULES:
        - Base EVERY claim on the provided data. Never invent essays, scores, or errors that are not present.
        - Use the saved current level and target to set the difficulty and explain the remaining gap.
        - Prioritize recurring or declining weak criteria; preserve strengths and reinforce criteria that are improving.
        - strengths: 2-4 items. Derive them from the highest recurring criterion scores and mastered vocabulary.
          Name the criterion explicitly (e.g. "Coherence and Cohesion is consistently your strongest criterion at 7.0").
        - weaknesses: 2-4 items. Derive them from the lowest criteria and the top recurring grammar categories.
          Name the criterion/category explicitly.
        - nextSteps: EXACTLY 3 items, ordered by expected impact on the band. Each step must be so specific the
          learner can start today (e.g. "Rewrite your 3 recent Task 2 introductions using only topic sentences", not
          "practice more"). Focus must be one of: {AllowedFocusValues}.
        - actionType/actionTarget: assign the SINGLE best action that executes this step. Prefer the most direct route:
          write_essay -> /write (append ?focus=<focus> and optionally the topic), review_history -> /history,
          practice_vocabulary -> /vocabulary, practice_quiz -> /quiz. Use "none" with an empty actionTarget
          only when no built-in screen fits the step.
        - explanationVi: EVERY strength, weakness, and step MUST carry a short Vietnamese (Tiếng Việt) sentence.
          Translate the jargon into everyday Vietnamese for a student (e.g. "Grammatical Range and Accuracy" ->
          "Ngữ pháp đa dạng và chính xác"). No English inside explanationVi.
        - recommendedTopic: pick a topic the learner has NOT already written about (or has written least), that
          naturally exercises the weakest criterion. Do not repeat the most frequent past topic. Provide 2-3
          ideaHints (each starting with "Point N:", a real argument the learner can develop) and 3-4 keyVocabulary
          topic-specific collocations/terms the learner should reuse in the essay.
        - All primary text in English; explanationVi always in Vietnamese. Keep every string reasonably short
          (titles < 12 words, descriptions < 30 words, explanationVi < 20 words).
        """;

    private readonly HttpClient _httpClient;
    private readonly IOptions<OpenRouterSettings> _openRouterOptions;
    private readonly ILogger<OpenRouterStudyGuideProvider> _logger;

    public OpenRouterStudyGuideProvider(
        IHttpClientFactory httpClientFactory,
        IOptions<OpenRouterSettings> openRouterOptions,
        ILogger<OpenRouterStudyGuideProvider> logger)
    {
        _httpClient = httpClientFactory.CreateClient("OpenRouter");
        _openRouterOptions = openRouterOptions;
        _logger = logger;

        if (!string.IsNullOrWhiteSpace(_openRouterOptions.Value.ApiKey) &&
            !_httpClient.DefaultRequestHeaders.Contains("Authorization"))
        {
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_openRouterOptions.Value.ApiKey}");
            _httpClient.DefaultRequestHeaders.Add("X-Title", "NomiWrite");
        }
    }

    public async Task<StudyGuideResult> GenerateGuideAsync(StudyGuideGenerationRequest request)
    {
        var prompt = BuildPrompt(request);
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
                max_tokens = 4096
            };

            for (var attempt = 1; attempt <= MaxRateLimitRetriesPerModel; attempt++)
            {
                using var response = await _httpClient.PostAsJsonAsync(endpoint, body);

                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        return await ParseGuideAsync(response);
                    }
                    catch (Exception exception) when (exception is InvalidOperationException or JsonException)
                    {
                        _logger.LogWarning(
                            exception,
                            "OpenRouter model {Model} returned an unusable study guide response; switching to the next free model.",
                            model);
                        failures.Add($"{model}: {exception.Message}");
                        break;
                    }
                }

                var errorPayload = await response.Content.ReadAsStringAsync();

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
                    "OpenRouter model {Model} unavailable ({StatusCode}): {Error}. Switching to the next free model.",
                    model,
                    (int)response.StatusCode,
                    errorPayload);
                failures.Add($"{model}: HTTP {(int)response.StatusCode}");
                break;
            }
        }

        _logger.LogError("All OpenRouter free models failed for study guide generation. Attempts: {Failures}", string.Join(" | ", failures));
        throw new HttpRequestException("Study guide generation is temporarily unavailable (no free AI model available).");
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

    private string BuildEndpoint()
    {
        var options = _openRouterOptions.Value;
        return string.IsNullOrWhiteSpace(options.Endpoint)
            ? "https://openrouter.ai/api/v1/chat/completions"
            : options.Endpoint;
    }

    private List<string> ResolveModels()
    {
        var configured = (_openRouterOptions.Value.Models ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(IsFreeModel)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (configured.Count == 0)
            configured.Add("openrouter/free");

        return configured;
    }

    /// <summary>
    /// Only models OpenRouter serves for free (a ":free" suffix, or the
    /// "openrouter/free" router) are ever called.
    /// </summary>
    private static bool IsFreeModel(string? model)
    {
        if (string.IsNullOrWhiteSpace(model))
            return false;

        var trimmed = model.Trim();
        return trimmed.EndsWith(":free", StringComparison.OrdinalIgnoreCase)
            || string.Equals(trimmed, "openrouter/free", StringComparison.OrdinalIgnoreCase);
    }

    private static async Task<StudyGuideResult> ParseGuideAsync(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        var text = ExtractText(content);

        if (string.IsNullOrWhiteSpace(text))
            throw new InvalidOperationException("OpenRouter returned an empty study guide response.");

        var guide = DeserializePayload(text);

        if (guide is null)
            throw new InvalidOperationException("OpenRouter returned an unparseable study guide response.");

        return new StudyGuideResult
        {
            Summary = guide.Summary ?? string.Empty,
            EstimatedBand = guide.EstimatedBand,
            Strengths = (guide.Strengths ?? new List<PayloadInsight>())
                .Select(s => new StudyGuideInsight
                {
                    Text = s.Text ?? string.Empty,
                    ExplanationVi = s.ExplanationVi ?? string.Empty
                })
                .ToList(),
            Weaknesses = (guide.Weaknesses ?? new List<PayloadInsight>())
                .Select(w => new StudyGuideInsight
                {
                    Text = w.Text ?? string.Empty,
                    ExplanationVi = w.ExplanationVi ?? string.Empty
                })
                .ToList(),
            NextSteps = guide.NextSteps ?? new List<StudyGuideStep>(),
            RecommendedTopic = guide.RecommendedTopic ?? new StudyGuideTopic()
        };
    }

    private static string BuildPrompt(StudyGuideGenerationRequest request)
    {
        var essaysSnapshot = JsonSerializer.Serialize(
            request.EssaySummaries.Select(e => new
            {
                overallBand = e.OverallBand,
                createdAt = e.CreatedAt,
                criteriaScores = e.CriteriaScores
            }),
            new JsonSerializerOptions { WriteIndented = true });

        var grammarSnapshot = JsonSerializer.Serialize(
            request.GrammarAggregates.Select(g => new
            {
                category = g.Category,
                count = g.Count,
                examples = g.Sentences
            }),
            new JsonSerializerOptions { WriteIndented = true });

        var vocabSnapshot = JsonSerializer.Serialize(
            request.Vocabulary.Select(v => new
            {
                originalWord = v.OriginalWord,
                suggestedWord = v.SuggestedWord,
                isMastered = v.IsMastered
            }),
            new JsonSerializerOptions { WriteIndented = true });

        var topicsSnapshot = JsonSerializer.Serialize(
            request.Topics.Select(t => new { title = t.Title }),
            new JsonSerializerOptions { WriteIndented = true });

        var builder = new StringBuilder();
        builder.AppendLine(SystemPrompt);
        builder.AppendLine();
        builder.AppendLine("=== INPUT DATA ===");
        builder.AppendLine();
        builder.AppendLine($"learnerProfile: {{ \"currentLevel\": \"{request.CurrentLevel ?? "unknown"}\", \"targetExam\": \"{request.TargetExam}\", \"targetBand\": {(request.TargetBand.HasValue ? request.TargetBand.Value.ToString("0.0") : "null")} }}");
        builder.AppendLine();
        builder.AppendLine("essaySummaries:");
        builder.AppendLine(essaysSnapshot);
        builder.AppendLine();
        builder.AppendLine("topics:");
        builder.AppendLine(topicsSnapshot);
        builder.AppendLine();
        builder.AppendLine("grammarAggregates:");
        builder.AppendLine(grammarSnapshot);
        builder.AppendLine();
        builder.AppendLine("vocabulary:");
        builder.AppendLine(vocabSnapshot);
        builder.AppendLine();
        builder.AppendLine("quizStats:");
        builder.AppendLine(request.QuizStats is null
            ? "null"
            : JsonSerializer.Serialize(new
            {
                attemptCount = request.QuizStats.AttemptCount,
                averageAccuracy = request.QuizStats.AverageAccuracy
            }, new JsonSerializerOptions { WriteIndented = true }));

        return builder.ToString();
    }

    private static string ExtractText(string content)
    {
        using var document = JsonDocument.Parse(content);
        var root = document.RootElement;

        if (root.TryGetProperty("error", out var error) &&
            error.TryGetProperty("message", out var errorMessage))
        {
            throw new InvalidOperationException($"OpenRouter error: {errorMessage.GetString()}");
        }

        if (!root.TryGetProperty("choices", out var choices))
            throw new InvalidOperationException("OpenRouter response missing 'choices'.");

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

    private static StudyGuidePayload? DeserializePayload(string text)
    {
        var json = text.Trim();

        if (json.StartsWith("```", StringComparison.Ordinal))
        {
            var start = json.IndexOf('{');
            var end = json.LastIndexOf('}');
            if (start >= 0 && end > start)
                json = json[start..(end + 1)];
        }

        return JsonSerializer.Deserialize<StudyGuidePayload>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    private sealed class StudyGuidePayload
    {
        public string? Summary { get; set; }
        public decimal EstimatedBand { get; set; }
        public List<PayloadInsight>? Strengths { get; set; }
        public List<PayloadInsight>? Weaknesses { get; set; }
        public List<StudyGuideStep>? NextSteps { get; set; }
        public StudyGuideTopic? RecommendedTopic { get; set; }
    }

    private sealed class PayloadInsight
    {
        public string? Text { get; set; }
        public string? ExplanationVi { get; set; }
    }
}
