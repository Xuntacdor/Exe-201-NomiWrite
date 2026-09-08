using System.Text.Json;
using FluentValidation;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NomiWrite.AICoordinator.Application.DTOs;
using NomiWrite.AICoordinator.Application.Exceptions;
using NomiWrite.AICoordinator.Application.Interfaces;
using NomiWrite.AICoordinator.Domain.Entities;
using NomiWrite.AICoordinator.Domain.Enums;
using NomiWrite.Shared.Contracts.Events.Grading;

namespace NomiWrite.AICoordinator.Application.Services;

public class GradingService : IGradingService
{
    private readonly IGradingDbContext _dbContext;
    private readonly IAiGradingProvider _aiGradingProvider;
    private readonly ISubscriptionStatusClient _subscriptionStatusClient;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<GradingService> _logger;
    private readonly IValidator<FlagGradingResultRequestDto> _flagFeedbackValidator;

    public GradingService(
        IGradingDbContext dbContext,
        IAiGradingProvider aiGradingProvider,
        ISubscriptionStatusClient subscriptionStatusClient,
        IPublishEndpoint publishEndpoint,
        ILogger<GradingService> logger,
        IValidator<FlagGradingResultRequestDto> flagFeedbackValidator)
    {
        _dbContext = dbContext;
        _aiGradingProvider = aiGradingProvider;
        _subscriptionStatusClient = subscriptionStatusClient;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
        _flagFeedbackValidator = flagFeedbackValidator;
    }

    public async Task GradeSubmissionAsync(Guid submissionId, Guid userId, string content)
    {
        var gradingResult = await _dbContext.GradingResults
            .FirstOrDefaultAsync(r => r.SubmissionId == submissionId && r.UserId == userId);

        if (gradingResult is null)
        {
            gradingResult = new GradingResult
            {
                SubmissionId = submissionId,
                UserId = userId,
                GrammarErrorsJson = "[]",
                VocabularySuggestionsJson = "[]",
                RestructuringSuggestionsJson = "[]",
                Status = GradingStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.GradingResults.Add(gradingResult);
            await _dbContext.SaveChangesAsync();
        }

        try
        {
            var aiResponse = await _aiGradingProvider.GradeEssayAsync(content);

            gradingResult.OverallBand = aiResponse.OverallBand;
            gradingResult.OverallFeedback = aiResponse.OverallFeedback;
            gradingResult.CriterionScores = aiResponse.Criteria.Select(c => new CriterionScore
            {
                CriterionName = c.Name,
                Score = c.Score,
                Comment = c.Comment
            }).ToList();
            gradingResult.GrammarErrorsJson = JsonSerializer.Serialize(aiResponse.GrammarErrors);
            gradingResult.VocabularySuggestionsJson = JsonSerializer.Serialize(aiResponse.VocabularySuggestions);
            gradingResult.RestructuringSuggestionsJson = JsonSerializer.Serialize(aiResponse.RestructuringSuggestions);
            gradingResult.Status = GradingStatus.Completed;
            gradingResult.CompletedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            await _publishEndpoint.Publish(new GradingCompletedEvent(
                submissionId,
                userId,
                gradingResult.OverallBand,
                gradingResult.CompletedAt.Value));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI grading failed for submission {SubmissionId}", submissionId);

            gradingResult.Status = GradingStatus.Failed;
            gradingResult.ErrorMessage = ex.Message;
            gradingResult.CompletedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task<GradingResultDto?> GetGradingResultBySubmissionIdAsync(Guid submissionId, Guid userId)
    {
        var result = await _dbContext.GradingResults
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.SubmissionId == submissionId && r.UserId == userId);

        if (result is null)
            return null;

        return ToGradingResultDto(result);
    }

    public async Task<IReadOnlyList<GradingHistoryItemDto>> GetGradingHistoryAsync(Guid userId)
    {
        return await _dbContext.GradingResults
            .AsNoTracking()
            .Where(r => r.UserId == userId && r.Status == GradingStatus.Completed)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new GradingHistoryItemDto
            {
                Id = r.Id,
                SubmissionId = r.SubmissionId,
                OverallBand = r.OverallBand,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<ComparisonDto> CompareWithPreviousAttemptAsync(Guid userId, Guid submissionId)
    {
        var currentResult = await _dbContext.GradingResults
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.SubmissionId == submissionId && r.UserId == userId);

        if (currentResult is null)
            throw new GradingResultNotFoundException(submissionId);

        var previousResult = await _dbContext.GradingResults
            .AsNoTracking()
            .Where(r =>
                r.UserId == userId &&
                r.Status == GradingStatus.Completed &&
                r.Id != currentResult.Id &&
                r.CreatedAt < currentResult.CreatedAt)
            .OrderByDescending(r => r.CreatedAt)
            .FirstOrDefaultAsync();

        return new ComparisonDto
        {
            Current = ToGradingResultDto(currentResult),
            Previous = previousResult is null ? null : ToGradingResultDto(previousResult),
            BandDifference = previousResult is null
                ? null
                : currentResult.OverallBand - previousResult.OverallBand
        };
    }

    public async Task<TutorReviewRequestDto> RequestTutorReviewAsync(
        Guid userId,
        Guid submissionId,
        string? accessToken)
    {
        var gradingResult = await _dbContext.GradingResults
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.SubmissionId == submissionId && r.UserId == userId);

        if (gradingResult is null)
            throw new GradingResultNotFoundException(submissionId);

        var subscription = await GetSubscriptionStatusOrDefaultAsync(userId, accessToken);

        // Human tutor review is VIP-gated: Free users get a 403 and the request is never created.
        if (!subscription.HasActiveSubscription)
            throw new TutorReviewSubscriptionRequiredException();

        var reviewRequest = new TutorReviewRequest
        {
            SubmissionId = submissionId,
            UserId = userId,
            Status = TutorReviewStatus.Pending,
            RequestedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.TutorReviewRequests.Add(reviewRequest);
        await _dbContext.SaveChangesAsync();

        return new TutorReviewRequestDto
        {
            Id = reviewRequest.Id,
            SubmissionId = reviewRequest.SubmissionId,
            Status = reviewRequest.Status,
            RequestedAt = reviewRequest.RequestedAt
        };
    }

    public async Task<IReadOnlyList<TutorReviewRequestDto>> GetTutorReviewRequestsAsync(Guid userId)
    {
        return await _dbContext.TutorReviewRequests
            .AsNoTracking()
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.RequestedAt)
            .Select(r => new TutorReviewRequestDto
            {
                Id = r.Id,
                SubmissionId = r.SubmissionId,
                Status = r.Status,
                RequestedAt = r.RequestedAt
            })
            .ToListAsync();
    }

    public async Task<FeedbackFlagConfirmationDto> FlagGradingResultAsync(
        Guid userId,
        Guid gradingResultId,
        FlagGradingResultRequestDto request)
    {
        var validationResult = await _flagFeedbackValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var gradingResult = await _dbContext.GradingResults
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == gradingResultId);

        if (gradingResult is null)
            throw new GradingResultByIdNotFoundException(gradingResultId);

        if (gradingResult.UserId != userId)
            throw new ForbiddenGradingResultAccessException();

        var flag = new GradingFeedbackFlag
        {
            GradingResultId = gradingResultId,
            UserId = userId,
            Reason = request.Reason,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.GradingFeedbackFlags.Add(flag);
        await _dbContext.SaveChangesAsync();

        return new FeedbackFlagConfirmationDto
        {
            Id = flag.Id,
            GradingResultId = flag.GradingResultId,
            Reason = flag.Reason,
            CreatedAt = flag.CreatedAt
        };
    }

    private async Task<SubscriptionStatusResult> GetSubscriptionStatusOrDefaultAsync(
        Guid userId,
        string? accessToken)
    {
        try
        {
            return await _subscriptionStatusClient.GetCurrentSubscriptionAsync(userId, accessToken);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(
                exception,
                "Failed to retrieve subscription status for user {UserId}; defaulting to no active subscription.",
                userId);

            return new SubscriptionStatusResult(false, null, null);
        }
    }

    private static GradingResultDto ToGradingResultDto(GradingResult result)
    {
        var grammarErrors = string.IsNullOrEmpty(result.GrammarErrorsJson)
            ? new List<GrammarErrorDto>()
            : JsonSerializer.Deserialize<List<GrammarErrorDto>>(result.GrammarErrorsJson) ?? new List<GrammarErrorDto>();

        var vocabularySuggestions = string.IsNullOrEmpty(result.VocabularySuggestionsJson)
            ? new List<VocabularySuggestionDto>()
            : JsonSerializer.Deserialize<List<VocabularySuggestionDto>>(result.VocabularySuggestionsJson)
                ?? new List<VocabularySuggestionDto>();

        var restructuringSuggestions = string.IsNullOrEmpty(result.RestructuringSuggestionsJson)
            ? new List<RestructuringSuggestionDto>()
            : JsonSerializer.Deserialize<List<RestructuringSuggestionDto>>(result.RestructuringSuggestionsJson)
                ?? new List<RestructuringSuggestionDto>();

        return new GradingResultDto
        {
            Id = result.Id,
            SubmissionId = result.SubmissionId,
            OverallBand = result.OverallBand,
            CriterionScores = result.CriterionScores.Select(c => new CriterionScoreDto
            {
                CriterionName = c.CriterionName,
                Score = c.Score,
                Comment = c.Comment
            }).ToList(),
            OverallFeedback = result.OverallFeedback,
            GrammarErrors = grammarErrors,
            VocabularySuggestions = vocabularySuggestions,
            RestructuringSuggestions = restructuringSuggestions,
            Status = result.Status
        };
    }
}
