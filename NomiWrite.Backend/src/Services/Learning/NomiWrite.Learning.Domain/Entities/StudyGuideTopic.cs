namespace NomiWrite.Learning.Domain.Entities;

/// <summary>
/// The next essay topic recommended for the learner. Stored inside the
/// RecommendedTopic JSON blob of a <see cref="StudyGuide"/>.
/// </summary>
public class StudyGuideTopic
{
    public string Title { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string SuggestedPrompt { get; set; } = string.Empty;
}