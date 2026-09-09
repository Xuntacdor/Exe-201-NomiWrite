using NomiWrite.AICoordinator.Application.DTOs;

namespace NomiWrite.AICoordinator.Application.Interfaces;

public interface IGradingService
{
    Task GradeSubmissionAsync(Guid submissionId, Guid userId, string content);
    Task<GradingResultDto?> GetGradingResultBySubmissionIdAsync(Guid submissionId, Guid userId);
    Task<IReadOnlyList<GradingHistoryItemDto>> GetGradingHistoryAsync(Guid userId);
    Task<ComparisonDto> CompareWithPreviousAttemptAsync(Guid userId, Guid submissionId);
    Task<TutorReviewRequestDto> RequestTutorReviewAsync(Guid userId, Guid submissionId, string? accessToken);
    Task<IReadOnlyList<TutorReviewRequestDto>> GetTutorReviewRequestsAsync(Guid userId);
    Task<FeedbackFlagConfirmationDto> FlagGradingResultAsync(Guid userId, Guid gradingResultId, FlagGradingResultRequestDto request);
}
