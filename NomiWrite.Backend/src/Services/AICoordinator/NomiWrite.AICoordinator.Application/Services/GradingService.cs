using System.Text.Json;
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
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<GradingService> _logger;

    public GradingService(
        IGradingDbContext dbContext,
        IAiGradingProvider aiGradingProvider,
        IPublishEndpoint publishEndpoint,
        ILogger<GradingService> logger)
    {
        _dbContext = dbContext;
        _aiGradingProvider = aiGradingProvider;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
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

    private static GradingResultDto ToGradingResultDto(GradingResult result)
    {
        var grammarErrors = string.IsNullOrEmpty(result.GrammarErrorsJson)
            ? new List<GrammarErrorDto>()
            : JsonSerializer.Deserialize<List<GrammarErrorDto>>(result.GrammarErrorsJson) ?? new List<GrammarErrorDto>();

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
            Status = result.Status
        };
    }
}
