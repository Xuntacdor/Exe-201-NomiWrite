namespace NomiWrite.User.Application.Interfaces;

public interface IWritingHistoryClient
{
    Task<IReadOnlyList<WritingSubmissionSummary>> GetSubmissionsAsync(Guid userId, string? accessToken);
}

public record WritingSubmissionSummary(Guid Id, DateTime SubmittedAt, string Status);
