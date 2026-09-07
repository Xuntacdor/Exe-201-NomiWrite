using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NomiWrite.AICoordinator.Application.DTOs;
using NomiWrite.AICoordinator.Application.Interfaces;
using NomiWrite.AICoordinator.Infrastructure.Options;

namespace NomiWrite.AICoordinator.Infrastructure.Services;

public class GeminiGradingProvider : IAiGradingProvider
{
    private readonly HttpClient _httpClient;
    private readonly GeminiSettings _settings;
    private readonly ILogger<GeminiGradingProvider> _logger;

    private const string GradingPrompt = """
        You are an experienced IELTS Writing examiner. Grade the following IELTS Writing essay based on the official IELTS scoring criteria.

        Score the essay on a scale of 0-9 for each of the following four criteria:
        1. Task Achievement - How well the essay addresses the task requirements, presents a clear position, and develops relevant ideas.
        2. Coherence and Cohesion - How well the essay is organized, paragraphed, and uses cohesive devices to connect ideas.
        3. Lexical Resource - The range and accuracy of vocabulary used, including paraphrasing and word choice.
        4. Grammatical Range and Accuracy - The variety and accuracy of grammatical structures used.

        Also provide:
        - An overall band score (calculate as the average of the four criteria, rounded to the nearest 0.5).
        - A short overall feedback paragraph (3-5 sentences) summarizing the essay's strengths and areas for improvement.
        - Up to 10 inline grammar or spelling errors found in the text. For each error, provide the exact original text, a suggested correction, and a brief one-line explanation.

        Be strict but fair in your grading. Use the standard IELTS band descriptors.
        """;

    public GeminiGradingProvider(HttpClient httpClient, IOptions<GeminiSettings> settings, ILogger<GeminiGradingProvider> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;

        _httpClient.DefaultRequestHeaders.Add("x-goog-api-key", _settings.ApiKey);
    }

    public async Task<GeminiGradingResponseSchema> GradeEssayAsync(string essayContent)
    {
        var requestUrl = _settings.Endpoint.Replace("{model}", _settings.Model);
        var requestBody = BuildRequestBody(essayContent);

        var response = await _httpClient.PostAsJsonAsync(requestUrl, requestBody);

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

    private object BuildRequestBody(string essayContent)
    {
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
                            text = $"{GradingPrompt}\n\n---\n\nESSAY:\n{essayContent}"
                        }
                    }
                }
            },
            generationConfig = new
            {
                responseMimeType = "application/json",
                responseSchema = new
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
                        }
                    },
                    required = new[] { "overallBand", "criteria", "overallFeedback", "grammarErrors" }
                }
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