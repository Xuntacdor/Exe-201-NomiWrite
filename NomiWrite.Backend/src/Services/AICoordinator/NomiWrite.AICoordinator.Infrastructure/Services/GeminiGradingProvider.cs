using System.Net.Http.Json;
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

public class GeminiGradingProvider : IAiGradingProvider
{
    private readonly HttpClient _httpClient;
    private readonly GeminiSettings _settings;
    private readonly IGradingDbContext _dbContext;
    private readonly IMemoryCache _cache;
    private readonly ILogger<GeminiGradingProvider> _logger;

    private const string ActiveConfigCacheKey = "ai_grading_active_config";
    private static readonly TimeSpan ActiveConfigCacheDuration = TimeSpan.FromSeconds(60);

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
        """;

    public GeminiGradingProvider(
        HttpClient httpClient,
        IOptions<GeminiSettings> settings,
        IGradingDbContext dbContext,
        IMemoryCache cache,
        ILogger<GeminiGradingProvider> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _dbContext = dbContext;
        _cache = cache;
        _logger = logger;

        _httpClient.DefaultRequestHeaders.Add("x-goog-api-key", _settings.ApiKey);
    }

    public async Task<GeminiGradingResponseSchema> GradeEssayAsync(string essayContent)
    {
        var config = await GetActiveConfigAsync();

        var modelName = config?.ModelName ?? _settings.Model;
        var systemPrompt = !string.IsNullOrWhiteSpace(config?.SystemPromptTemplate)
            ? config!.SystemPromptTemplate!
            : DefaultGradingPrompt;

        var endpoint = _settings.Endpoint.Replace("{model}", modelName);
        var requestBody = BuildRequestBody(essayContent, systemPrompt, config);

        var response = await _httpClient.PostAsJsonAsync(endpoint, requestBody);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            _logger.LogError("Gemini API returned {StatusCode}: {ErrorBody}", response.StatusCode, errorBody);
            throw new HttpRequestException($"Gemini API error: {(int)response.StatusCode} - {errorBody}");
        }

        var geminiResponse = await response.Content.ReadFromJsonAsync<GeminiApiResponse>();

        var text = geminiResponse?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text
            ?? throw new InvalidOperationException("Gemini response did not contain any text content.");

        var gradingResponse = JsonSerializer.Deserialize<GeminiGradingResponseSchema>(text,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? throw new InvalidOperationException("Failed to deserialize Gemini grading response.");

        return gradingResponse;
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

    private object BuildRequestBody(string essayContent, string systemPrompt, AiGradingConfig? config)
    {
        var generationConfig = new Dictionary<string, object>
        {
            ["responseMimeType"] = "application/json",
            ["responseSchema"] = BuildResponseSchema()
        };

        if (config?.Temperature.HasValue == true)
            generationConfig["temperature"] = config.Temperature.Value;

        if (config?.MaxOutputTokens.HasValue == true)
            generationConfig["maxOutputTokens"] = config.MaxOutputTokens.Value;

        return new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new
                        {
                            text = $"{systemPrompt}\n\n---\n\nESSAY:\n{essayContent}"
                        }
                    }
                }
            },
            generationConfig
        };
    }

    private object BuildResponseSchema()
    {
        return new
        {
            type = "object",
            properties = new
            {
                overallBand = new { type = "number" },
                criteria = new
                {
                    type = "array",
                    items = new
                    {
                        type = "object",
                        properties = new
                        {
                            name = new { type = "string" },
                            score = new { type = "number" },
                            comment = new { type = "string" }
                        },
                        required = new[] { "name", "score", "comment" }
                    }
                },
                overallFeedback = new { type = "string" },
                grammarErrors = new
                {
                    type = "array",
                    items = new
                    {
                        type = "object",
                        properties = new
                        {
                            originalText = new { type = "string" },
                            suggestion = new { type = "string" },
                            explanation = new { type = "string" }
                        },
                        required = new[] { "originalText", "suggestion", "explanation" }
                    }
                },
                vocabularySuggestions = new
                {
                    type = "array",
                    items = new
                    {
                        type = "object",
                        properties = new
                        {
                            originalWord = new { type = "string" },
                            suggestedAlternatives = new
                            {
                                type = "array",
                                items = new { type = "string" }
                            },
                            context = new { type = "string" }
                        },
                        required = new[] { "originalWord", "suggestedAlternatives", "context" }
                    }
                },
                restructuringSuggestions = new
                {
                    type = "array",
                    items = new
                    {
                        type = "object",
                        properties = new
                        {
                            originalSentence = new { type = "string" },
                            suggestedRewrite = new { type = "string" },
                            reason = new { type = "string" }
                        },
                        required = new[] { "originalSentence", "suggestedRewrite", "reason" }
                    }
                }
            },
            required = new[]
            {
                "overallBand",
                "criteria",
                "overallFeedback",
                "grammarErrors",
                "vocabularySuggestions",
                "restructuringSuggestions"
            }
        };
    }

    private sealed class GeminiApiResponse
    {
        public List<GeminiCandidate>? Candidates { get; set; }
    }

    private sealed class GeminiCandidate
    {
        public GeminiContent? Content { get; set; }
    }

    private sealed class GeminiContent
    {
        public List<GeminiPart>? Parts { get; set; }
    }

    private sealed class GeminiPart
    {
        public string? Text { get; set; }
    }
}