using NomiWrite.AICoordinator.Application.DTOs;

namespace NomiWrite.AICoordinator.Application.Interfaces;

public interface IAiGradingProvider
{
    Task<GeminiGradingResponseSchema> GradeEssayAsync(string essayContent);

    void InvalidateActiveConfigCache();
}
