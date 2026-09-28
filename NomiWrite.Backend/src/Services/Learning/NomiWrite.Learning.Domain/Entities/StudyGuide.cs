namespace NomiWrite.Learning.Domain.Entities;

/// <summary>
/// The stored, personalized study guide (improvement roadmap) produced by the
/// LLM from the user's writing history. One row per user — the latest row is
/// the cached guide. A newly completed grading invalidates this row so the
/// next Guide visit rebuilds the roadmap from the latest learning signals.
/// </summary>
public class StudyGuide : Common.BaseEntity
{
    public Guid UserId { get; set; }
    public string TargetExam { get; set; } = string.Empty;
    public decimal? TargetBand { get; set; }
    public string Summary { get; set; } = string.Empty;
    public decimal EstimatedBand { get; set; }
    public List<StudyGuideInsight> Strengths { get; set; } = new();
    public List<StudyGuideInsight> Weaknesses { get; set; } = new();
    public List<StudyGuideStep> NextSteps { get; set; } = new();
    public StudyGuideTopic RecommendedTopic { get; set; } = new();
    public int AnalyzedEssayCount { get; set; }
}
