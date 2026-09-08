namespace NomiWrite.Shared.Contracts.Events.Grading;

public sealed record GradingCompletedEvent(
    Guid SubmissionId,
    Guid UserId,
    decimal OverallBand,
    DateTime CompletedAt);
