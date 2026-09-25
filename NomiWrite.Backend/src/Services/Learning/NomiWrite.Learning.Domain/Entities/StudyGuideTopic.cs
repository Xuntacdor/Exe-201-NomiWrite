namespace NomiWrite.Learning.Domain.Entities;

/// <summary>
/// The next essay topic recommended for the learner. Stored inside the
/// RecommendedTopic JSON blob of a <see cref="StudyGuide"/>. IdeaHints are
/// brief brainstorming arguments and KeyVocabulary are topic collocations so
/// the learner can start writing without a blank-page struggle.
/// </summary>
public class StudyGuideTopic
{
    public string Title { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string SuggestedPrompt { get; set; } = string.Empty;
    public List<string> IdeaHints { get; set; } = new();
    public List<string> KeyVocabulary { get; set; } = new();
}