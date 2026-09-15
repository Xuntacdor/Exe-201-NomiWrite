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

public class GeminiQuizProvider : IAiQuizProvider
{
    private const string FixedCategoryList =
        "Mạo từ; Hòa hợp chủ ngữ - động từ; Chia thì động từ; Số ít / số nhiều của danh từ; " +
        "Giới từ; Câu điều kiện; Mệnh đề quan hệ; Câu bị động; Trật tự từ trong câu; " +
        "Liên từ và từ nối; Đại từ; So sánh hơn / so sánh nhất; Động từ khuyết thiếu; " +
        "Danh động từ và động từ nguyên mẫu; Cấu trúc câu; Dùng từ sai loại; " +
        "Lặp từ / diễn đạt dài dòng; Dấu câu; Khác";

    private const string AllowedTypes = "multiple_choice, fill_blank, rewrite";

    private readonly HttpClient _httpClient;
    private readonly IOptions<GeminiSettings> _geminiOptions;
    private readonly ILogger<GeminiQuizProvider> _logger;

    public GeminiQuizProvider(
        IHttpClientFactory httpClientFactory,
        IOptions<GeminiSettings> geminiOptions,
        ILogger<GeminiQuizProvider> logger)
    {
        _httpClient = httpClientFactory.CreateClient("Gemini");
        _geminiOptions = geminiOptions;
        _logger = logger;
    }

    public async Task<List<QuizQuestionItem>> GenerateQuestionsAsync(
        QuizGenerationRequest request,
        int targetCount)
    {
        var prompt = BuildPrompt(request, targetCount);

        var body = new
        {
            contents = new[]
            {
                new { parts = new[] { new { text = prompt } } }
            },
            generationConfig = new
            {
                temperature = 0.7,
                maxOutputTokens = 2048
            }
        };

        using var response = await _httpClient.PostAsJsonAsync(_geminiOptions.Value.Endpoint, body);

        if (!response.IsSuccessStatusCode)
        {
            var errorPayload = await response.Content.ReadAsStringAsync();
            _logger.LogWarning(
                "Gemini quiz generation returned {StatusCode}: {Error}",
                response.StatusCode,
                errorPayload);
            throw new HttpRequestException($"Gemini returned {(int)response.StatusCode}.");
        }

        var content = await response.Content.ReadAsStringAsync();
        var text = ExtractText(content);

        if (string.IsNullOrWhiteSpace(text))
            throw new InvalidOperationException("Gemini returned an empty quiz response.");

        var questions = DeserializePayload(text);

        if (questions?.Count == 0)
            throw new InvalidOperationException("Gemini returned no quiz questions.");

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