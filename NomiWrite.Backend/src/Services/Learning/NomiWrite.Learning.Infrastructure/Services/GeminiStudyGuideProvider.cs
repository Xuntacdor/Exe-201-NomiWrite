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

public class GeminiStudyGuideProvider : IStudyGuideAiProvider
{
    private const string AllowedFocusValues = "grammar, vocabulary, coherence, task_response, lexical";
    private const string AllowedActionTypes = "write_essay, review_history, practice_vocabulary, practice_quiz, none";

    private const int MaxAttempts = 5;
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
        - target: the exam/type the learner is preparing for and the target band score (may be absent).
        - essaySummaries: the most recent graded essays. Each contains an overallBand, a createdAt date,
          and criteriaScores (e.g. Task Achievement, Coherence and Cohesion, Lexical Resource,
          Grammatical Range and Accuracy).
        - topics: the essay topics this learner has already written about.
        - grammarAggregates: recurring grammar error categories with occurrence counts and example sentences.
        - vocabulary: recently suggested word upgrades, each marked with whether the learner already mastered it.
        - quizStats: optional accuracy of the learner's recent practice quizzes.

        TASK:
        Diagnose the learner across the four official writing criteria, then emit a STRICT JSON response
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
    private readonly IOptions<GeminiSettings> _geminiOptions;
    private readonly ILogger<GeminiStudyGuideProvider> _logger;

    public GeminiStudyGuideProvider(
        IHttpClientFactory httpClientFactory,
        IOptions<GeminiSettings> geminiOptions,
        ILogger<GeminiStudyGuideProvider> logger)
    {
        _httpClient = httpClientFactory.CreateClient("Gemini");
        _geminiOptions = geminiOptions;
        _logger = logger;

        if (!string.IsNullOrWhiteSpace(_geminiOptions.Value.ApiKey) &&
            !_httpClient.DefaultRequestHeaders.Contains("x-goog-api-key"))
        {
            _httpClient.DefaultRequestHeaders.Add("x-goog-api-key", _geminiOptions.Value.ApiKey);
        }
    }

    public async Task<StudyGuideResult> GenerateGuideAsync(StudyGuideGenerationRequest request)
    {
        var prompt = BuildPrompt(request);

        var body = new
        {
            contents = new[]
            {
                new { parts = new[] { new { text = prompt } } }
            },
            generationConfig = new
            {
                temperature = 0.7,
                maxOutputTokens = 4096
            }
        };

        var endpoint = BuildEndpoint();

        for (var attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            using var response = await _httpClient.PostAsJsonAsync(endpoint, body);

            if (response.IsSuccessStatusCode)
                return await ParseGuideAsync(response);

            var errorPayload = await response.Content.ReadAsStringAsync();
            if (IsTransientGeminiError(response.StatusCode) && attempt < MaxAttempts)
            {
                _logger.LogWarning(
                    "Gemini study guide generation returned transient {StatusCode} on attempt {Attempt}/{MaxAttempts}; retrying.",
                    response.StatusCode,
                    attempt,
                    MaxAttempts);
                await DelayBackoffAsync(GetBackoff(attempt), CancellationToken.None);
                continue;
            }

            _logger.LogWarning(
                "Gemini study guide generation returned {StatusCode}: {Error}",
                response.StatusCode,
                errorPayload);
            throw new HttpRequestException($"Gemini returned {(int)response.StatusCode}.");
        }

        throw new InvalidOperationException("Gemini study guide generation failed after all retry attempts.");
    }

    // Transient Gemini overloads (429 / 503) typically clear within seconds, so
    // back off exponentially (2s, 4s, 8s, 16s) with jitter instead of giving up
    // after two quick retries.
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

    private string BuildEndpoint()
    {
        var options = _geminiOptions.Value;
        var endpoint = string.IsNullOrWhiteSpace(options.Endpoint)
            ? "https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent"
            : options.Endpoint;
        var model = string.IsNullOrWhiteSpace(options.Model) ? "gemini-3.6-flash" : options.Model;

        return endpoint.Replace("{model}", model, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsTransientGeminiError(HttpStatusCode statusCode) =>
        statusCode is HttpStatusCode.ServiceUnavailable or HttpStatusCode.TooManyRequests;

    private static async Task<StudyGuideResult> ParseGuideAsync(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        var text = ExtractText(content);

        if (string.IsNullOrWhiteSpace(text))
            throw new InvalidOperationException("Gemini returned an empty study guide response.");

        var guide = DeserializePayload(text);

        if (guide is null)
            throw new InvalidOperationException("Gemini returned an unparseable study guide response.");

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
        builder.AppendLine($"target: {{ \"exam\": \"{request.TargetExam}\", \"band\": {(request.TargetBand.HasValue ? request.TargetBand.Value.ToString("0.0") : "null")} }}");
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

        if (!root.TryGetProperty("candidates", out var candidates))
            throw new InvalidOperationException("Gemini response missing 'candidates'.");

        var textBuilder = new StringBuilder();

        foreach (var candidate in candidates.EnumerateArray())
        {
            if (candidate.TryGetProperty("content", out var contentProperty) &&
                contentProperty.TryGetProperty("parts", out var parts))
            {
                foreach (var part in parts.EnumerateArray())
                {
                    if (part.TryGetProperty("text", out var text))
                        textBuilder.AppendLine(text.GetString());
                }
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