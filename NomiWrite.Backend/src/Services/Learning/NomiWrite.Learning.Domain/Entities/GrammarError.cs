namespace NomiWrite.Learning.Domain.Entities;

/// <summary>
/// A grammar weakness extracted by the AI grading cycle. Stored locally by the
/// Learning service so personalized quizzes can be generated from the user's
/// real weak points without additional cross-service round trips.
/// </summary>
public class GrammarError : Common.BaseEntity
{
    public Guid UserId { get; set; }
    public Guid SubmissionId { get; set; }
    public string GrammarCategory { get; set; } = string.Empty;
    public string Sentence { get; set; } = string.Empty;
    public string ErrorPart { get; set; } = string.Empty;
    public string Suggestion { get; set; } = string.Empty;
    public string Explanation { get; set; } = string.Empty;
}