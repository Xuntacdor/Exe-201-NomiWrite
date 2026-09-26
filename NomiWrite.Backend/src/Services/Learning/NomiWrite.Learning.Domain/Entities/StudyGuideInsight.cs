namespace NomiWrite.Learning.Domain.Entities;

/// <summary>
/// A single strength or weakness in a <see cref="StudyGuide"/>. Carries the
/// plain-English statement plus a short Vietnamese explanation so Vietnamese
/// learners understand the academic phrasing without mental fatigue.
/// </summary>
public class StudyGuideInsight
{
    public string Text { get; set; } = string.Empty;
    public string ExplanationVi { get; set; } = string.Empty;
}