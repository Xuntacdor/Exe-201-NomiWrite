using NomiWrite.Learning.Application.DTOs;

namespace NomiWrite.Learning.Application.Interfaces;

/// <summary>
/// Fetches the compact essay-history signals needed to build a study guide:
/// graded criterion scores (AICoordinator) and the essay topics actually
/// written about (Writing service). Keeps raw essays out of the prompt.
/// </summary>
public interface IEssayHistoryClient
{
    Task<IReadOnlyList<GradedEssaySummaryDto>> GetGradingHistoryAsync(Guid userId, string? accessToken);

    Task<IReadOnlyList<EssayTopicDto>> GetWrittenTopicsAsync(Guid userId, string? accessToken);
}