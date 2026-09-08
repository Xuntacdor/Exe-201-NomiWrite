using NomiWrite.Writing.Domain.Common;
using NomiWrite.Writing.Domain.Enums;

namespace NomiWrite.Writing.Domain.Entities;

public class WritingSubmission : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid WritingPromptId { get; set; }
    public string Content { get; set; } = string.Empty;
    public int WordCount { get; set; }
    public bool IsTimed { get; set; }
    public DateTime? DeadlineAt { get; set; }
    public bool SubmittedLate { get; set; }
    public SubmissionStatus Status { get; set; } = SubmissionStatus.Draft;
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? SubmittedAt { get; set; }

    public WritingPrompt? WritingPrompt { get; set; }
}