namespace NomiWrite.User.Application.Interfaces;

public interface IGradingHistoryClient
{
    Task<IReadOnlyList<GradingHistoryEntry>> GetHistoryAsync(Guid userId, string? accessToken);
}

public record GradingHistoryEntry(Guid Id, Guid SubmissionId, decimal OverallBand, DateTime CreatedAt, Dictionary<string, decimal> CriteriaScores);
