using NomiWrite.AICoordinator.Application.DTOs;

namespace NomiWrite.AICoordinator.Application.Interfaces;

public interface IGradingService
{
    Task GradeSubmissionAsync(Guid submissionId, Guid userId, string content);
    Task<GradingResultDto?> GetGradingResultBySubmissionIdAsync(Guid submissionId, Guid userId);
}
