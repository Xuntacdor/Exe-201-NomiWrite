namespace NomiWrite.Learning.Domain.Entities;

/// <summary>
/// A single actionable step inside a personalized <see cref="StudyGuide"/>.
/// Stored inside the NextSteps JSON blob. Focus is one of:
/// grammar | vocabulary | coherence | task_response | lexical.
/// ActionType is one of: write_essay | review_history | practice_vocabulary |
/// practice_quiz | none, and ActionTarget is a relative app route the UI can
/// deep-link to so the learner completes the step in one click.
/// </summary>
public class StudyGuideStep
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Focus { get; set; } = string.Empty;
    public string ExplanationVi { get; set; } = string.Empty;
    public string ActionType { get; set; } = string.Empty;
    public string ActionTarget { get; set; } = string.Empty;
}