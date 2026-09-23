namespace NomiWrite.Learning.Domain.Entities;

/// <summary>
/// A single actionable step inside a personalized <see cref="StudyGuide"/>.
/// Stored inside the NextSteps JSON blob. Focus is one of:
/// grammar | vocabulary | coherence | task_response | lexical.
/// </summary>
public class StudyGuideStep
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Focus { get; set; } = string.Empty;
}