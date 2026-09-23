using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NomiWrite.Learning.Application.DTOs;
using NomiWrite.Learning.Application.Exceptions;
using NomiWrite.Learning.Application.Interfaces;
using NomiWrite.Learning.Domain.Entities;

namespace NomiWrite.Learning.Application.Services;

public class StudyGuideService : IStudyGuideService
{
    private const int MaxEssaysAnalyzed = 15;
    private const int MaxGrammarCategories = 8;
    private const int MaxGrammarExamplesPerCategory = 2;
    private const int MaxVocabularySnapshots = 10;
    private const int MaxTopics = 20;

    private readonly ILearningDbContext _dbContext;
    private readonly IEssayHistoryClient _essayHistoryClient;
    private readonly IStudyGuideAiProvider _aiProvider;
    private readonly IFallbackStudyGuideProvider _fallbackProvider;
    private readonly IValidator<GenerateStudyGuideRequestDto> _requestValidator;
    private readonly ILogger<StudyGuideService> _logger;

    public StudyGuideService(
        ILearningDbContext dbContext,
        IEssayHistoryClient essayHistoryClient,
        IStudyGuideAiProvider aiProvider,
        IFallbackStudyGuideProvider fallbackProvider,
        IValidator<GenerateStudyGuideRequestDto> requestValidator,
        ILogger<StudyGuideService> logger)
    {
        _dbContext = dbContext;
        _essayHistoryClient = essayHistoryClient;
        _aiProvider = aiProvider;
        _fallbackProvider = fallbackProvider;
        _requestValidator = requestValidator;
        _logger = logger;
    }

    public async Task<StudyGuideDto?> GetLatestGuideAsync(Guid userId)
    {
        var guide = await _dbContext.StudyGuides
            .AsNoTracking()
            .Where(g => g.UserId == userId)
            .OrderByDescending(g => g.CreatedAt)
            .FirstOrDefaultAsync();

        return guide is null ? null : ToDto(guide);
    }

    public async Task<StudyGuideDto> GenerateGuideAsync(
        Guid userId,
        GenerateStudyGuideRequestDto request,
        string? accessToken)
    {
        var validationResult = await _requestValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var essaySummaries = await FetchGradingHistoryAsync(userId, accessToken);
        var topics = await FetchWrittenTopicsAsync(userId, accessToken);
        var grammarAggregates = await CollectGrammarAggregatesAsync(userId);
        var vocabulary = await CollectVocabularyAsync(userId);
        var quizStats = await CollectQuizStatsAsync(userId);

        var hasAnalyzeableData = essaySummaries.Count > 0 ||
                                 grammarAggregates.Count > 0 ||
                                 vocabulary.Count > 0;

        if (!hasAnalyzeableData)
            throw new StudyGuideInsufficientDataException();

        var generationRequest = new StudyGuideGenerationRequest
        {
            TargetExam = string.IsNullOrWhiteSpace(request.TargetExam)
                ? "IELTS Academic - Writing Task 2"
                : request.TargetExam.Trim(),
            TargetBand = request.TargetBand,
            EssaySummaries = essaySummaries
                .OrderByDescending(e => e.CreatedAt)
                .Take(MaxEssaysAnalyzed)
                .ToList(),
            Topics = topics.Take(MaxTopics).ToList(),
            GrammarAggregates = grammarAggregates,
            Vocabulary = vocabulary.Take(MaxVocabularySnapshots).ToList(),
            QuizStats = quizStats
        };

        var result = await GenerateGuideAsync(generationRequest);

        var now = DateTime.UtcNow;

        // Cache/idempotency: keep a single latest row per user so the Guide page
        // is served from the database instead of re-running the paid LLM call.
        var existing = await _dbContext.StudyGuides
            .Where(g => g.UserId == userId)
            .ToListAsync();

        if (existing.Count > 0)
            _dbContext.StudyGuides.RemoveRange(existing);

        var guide = new StudyGuide
        {
            UserId = userId,
            TargetExam = generationRequest.TargetExam,
            TargetBand = request.TargetBand,
            Summary = result.Summary,
            EstimatedBand = result.EstimatedBand,
            Strengths = result.Strengths,
            Weaknesses = result.Weaknesses,
            NextSteps = result.NextSteps,
            RecommendedTopic = result.RecommendedTopic,
            AnalyzedEssayCount = generationRequest.EssaySummaries.Count,
            CreatedAt = now
        };

        _dbContext.StudyGuides.Add(guide);
        await _dbContext.SaveChangesAsync();

        return ToDto(guide);
    }

    private async Task<StudyGuideResult> GenerateGuideAsync(StudyGuideGenerationRequest request)
    {
        try
        {
            return NormalizeResult(await _aiProvider.GenerateGuideAsync(request), request);
        }
        catch (Exception exception) when (exception is HttpRequestException
            or InvalidOperationException
            or TaskCanceledException
            or System.Text.Json.JsonException)
        {
            _logger.LogWarning(exception, "AI study guide generation failed; using deterministic fallback.");
        }

        return NormalizeResult(_fallbackProvider.GenerateGuide(request), request);
    }

    private static StudyGuideResult NormalizeResult(StudyGuideResult result, StudyGuideGenerationRequest request)
    {
        var strengths = result.Strengths
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Distinct()
            .Take(4)
            .ToList();

        var weaknesses = result.Weaknesses
            .Where(w => !string.IsNullOrWhiteSpace(w))
            .Distinct()
            .Take(4)
            .ToList();

        var steps = result.NextSteps
            .Where(s => !string.IsNullOrWhiteSpace(s.Title))
            .Take(3)
            .ToList();

        var topic = result.RecommendedTopic;
        if (string.IsNullOrWhiteSpace(topic.Title))
        {
            topic = new StudyGuideTopic
            {
                Title = BuildFallbackTopicTitle(request),
                Reason = "Practice writing in a controlled setting to turn your weakest criterion into a strength.",
                SuggestedPrompt = "Write an IELTS Writing Task 2 essay that directly addresses the prompt and develops each idea with a clear topic sentence."
            };
        }

        var estimatedBand = result.EstimatedBand;
        if (estimatedBand <= 0 && request.EssaySummaries.Count > 0)
        {
            estimatedBand = RoundToHalf(
                request.EssaySummaries.Average(e => e.OverallBand));
        }

        return new StudyGuideResult
        {
            Summary = string.IsNullOrWhiteSpace(result.Summary)
                ? BuildFallbackSummary(request, estimatedBand)
                : result.Summary.Trim(),
            EstimatedBand = estimatedBand,
            Strengths = strengths,
            Weaknesses = weaknesses,
            NextSteps = steps,
            RecommendedTopic = topic
        };
    }

    private static string BuildFallbackSummary(StudyGuideGenerationRequest request, decimal estimatedBand)
    {
        return request.EssaySummaries.Count > 0
            ? $"Your recent writing averages band {estimatedBand:0.0}. Keep practicing the specific weaknesses below to close the gap toward your target."
            : "Keep writing consistently; your next essay will unlock more precise guidance.";
    }

    private static string BuildFallbackTopicTitle(StudyGuideGenerationRequest request)
    {
        if (request.Topics.Count == 0)
            return "IELTS Writing Task 2 - Opinion essay";

        var pivot = request.Topics[0];
        return $"A new {request.TargetExam} essay (e.g. extending \"{pivot.Title}\" to a fresh angle)";
    }

    private static decimal RoundToHalf(decimal value) =>
        Math.Round(value * 2, MidpointRounding.AwayFromZero) / 2;

    private async Task<IReadOnlyList<GradedEssaySummaryDto>> FetchGradingHistoryAsync(Guid userId, string? accessToken)
    {
        try
        {
            return await _essayHistoryClient.GetGradingHistoryAsync(userId, accessToken);
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException)
        {
            _logger.LogWarning(exception,
                "Failed to load grading history for study guide of user {UserId}; continuing with local weak points only.",
                userId);
            return Array.Empty<GradedEssaySummaryDto>();
        }
    }

    private async Task<IReadOnlyList<EssayTopicDto>> FetchWrittenTopicsAsync(Guid userId, string? accessToken)
    {
        try
        {
            return await _essayHistoryClient.GetWrittenTopicsAsync(userId, accessToken);
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException)
        {
            _logger.LogWarning(exception,
                "Failed to load written topics for study guide of user {UserId}; continuing without topic signal.",
                userId);
            return Array.Empty<EssayTopicDto>();
        }
    }

    private async Task<List<GrammarAggregateDto>> CollectGrammarAggregatesAsync(Guid userId)
    {
        var recentErrors = await _dbContext.GrammarErrors
            .AsNoTracking()
            .Where(g => g.UserId == userId)
            .OrderByDescending(g => g.CreatedAt)
            .Take(100)
            .ToListAsync();

        return recentErrors
            .GroupBy(g => string.IsNullOrWhiteSpace(g.GrammarCategory) ? "Khác" : g.GrammarCategory)
            .OrderByDescending(g => g.Count())
            .Take(MaxGrammarCategories)
            .Select(g => new GrammarAggregateDto
            {
                Category = g.Key,
                Count = g.Count(),
                Sentences = g
                    .Select(e => e.Sentence)
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Take(MaxGrammarExamplesPerCategory)
                    .ToList()
            })
            .ToList();
    }

    private async Task<List<VocabSnapshotDto>> CollectVocabularyAsync(Guid userId)
    {
        var recentVocab = await _dbContext.VocabSuggestions
            .AsNoTracking()
            .Where(v => v.UserId == userId)
            .OrderByDescending(v => v.CreatedAt)
            .Take(200)
            .ToListAsync();

        return recentVocab
            .OrderBy(v => v.IsMastered)
            .ThenByDescending(v => v.CreatedAt)
            .Select(v => new VocabSnapshotDto
            {
                OriginalWord = v.OriginalWord,
                SuggestedWord = v.SuggestedWord,
                IsMastered = v.IsMastered
            })
            .ToList();
    }

    private async Task<QuizStatsDto?> CollectQuizStatsAsync(Guid userId)
    {
        var attempts = await _dbContext.QuizAttempts
            .AsNoTracking()
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.AttemptedAt)
            .Take(200)
            .ToListAsync();

        if (attempts.Count == 0)
            return null;

        var answered = attempts.Sum(a => a.TotalQuestions);
        var correct = attempts.Sum(a => a.Score);

        return new QuizStatsDto
        {
            AttemptCount = attempts.Count,
            AverageAccuracy = answered > 0
                ? Math.Round(correct * 100m / answered, 1)
                : 0
        };
    }

    private static StudyGuideDto ToDto(StudyGuide guide)
    {
        return new StudyGuideDto
        {
            Id = guide.Id,
            UserId = guide.UserId,
            TargetExam = guide.TargetExam,
            TargetBand = guide.TargetBand,
            Summary = guide.Summary,
            EstimatedBand = guide.EstimatedBand,
            Strengths = guide.Strengths,
            Weaknesses = guide.Weaknesses,
            NextSteps = guide.NextSteps
                .Select(s => new StudyGuideStepDto
                {
                    Title = s.Title,
                    Description = s.Description,
                    Focus = s.Focus
                })
                .ToList(),
            RecommendedTopic = new StudyGuideTopicDto
            {
                Title = guide.RecommendedTopic.Title,
                Reason = guide.RecommendedTopic.Reason,
                SuggestedPrompt = guide.RecommendedTopic.SuggestedPrompt
            },
            AnalyzedEssayCount = guide.AnalyzedEssayCount,
            CreatedAt = guide.CreatedAt
        };
    }
}