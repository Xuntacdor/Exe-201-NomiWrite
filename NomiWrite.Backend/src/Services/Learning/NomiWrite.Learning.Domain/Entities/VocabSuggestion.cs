namespace NomiWrite.Learning.Domain.Entities;

/// <summary>
/// A word upgrade suggested by the AI grading cycle for the user's personal
/// vocabulary notebook. Each row links a weak/repetitive word to a stronger
/// alternative, scoped to the originating submission.
/// </summary>
public class VocabSuggestion : Common.BaseEntity
{
    public Guid UserId { get; set; }
    public Guid? SubmissionId { get; set; }
    public string Topic { get; set; } = string.Empty;
    public string OriginalWord { get; set; } = string.Empty;
    public string SuggestedWord { get; set; } = string.Empty;
    public string ExampleSentence { get; set; } = string.Empty;
    public bool IsMastered { get; set; }
}