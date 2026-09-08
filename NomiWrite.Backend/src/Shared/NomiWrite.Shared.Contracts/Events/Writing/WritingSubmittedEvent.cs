namespace NomiWrite.Shared.Contracts.Events.Writing;

/// <summary>
/// Published by Writing Service when a submission is finalized.
/// Consumed by the future AI grading service.
/// </summary>
public sealed record WritingSubmittedEvent(
    Guid SubmissionId,
    Guid UserId,
    Guid WritingPromptId,
    string Content,
    int WordCount,
    DateTime SubmittedAt);