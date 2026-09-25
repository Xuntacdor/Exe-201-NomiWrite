using NomiWrite.Learning.Domain.Entities;

namespace NomiWrite.Learning.Application.DTOs;

public class GenerateStudyGuideRequestDto
{
    /// <summary>Target exam label, e.g. "IELTS Academic - Writing Task 2".</summary>
    public string? TargetExam { get; set; }

    /// <summary>Target IELTS band score (1.0 - 9.0). Optional.</summary>
    public decimal? TargetBand { get; set; }

    /// <summary>
    /// When true, the AI analysis is re-run even if a fresh cached guide exists.
    /// </summary>
    public bool ForceRefresh { get; set; }
}

/// <summary>
/// The stored guide returned to the client. Mirrors <see cref="StudyGuide"/>.
/// </summary>
public class StudyGuideDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string TargetExam { get; set; } = string.Empty;
    public decimal? TargetBand { get; set; }
    public string Summary { get; set; } = string.Empty;
    public decimal EstimatedBand { get; set; }
    public List<StudyGuideInsightDto> Strengths { get; set; } = new();
    public List<StudyGuideInsightDto> Weaknesses { get; set; } = new();
    public List<StudyGuideStepDto> NextSteps { get; set; } = new();
    public StudyGuideTopicDto RecommendedTopic { get; set; } = new();
    public int AnalyzedEssayCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// A strength/weakness statement plus a short Vietnamese plain-language
/// breakdown so Vietnamese learners grasp it without juggling academic jargon.
/// </summary>
public class StudyGuideInsightDto
{
    public string Text { get; set; } = string.Empty;
    public string ExplanationVi { get; set; } = string.Empty;
}

public class StudyGuideStepDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Focus { get; set; } = string.Empty;
    public string ExplanationVi { get; set; } = string.Empty;

    /// <summary>
    /// One of write_essay | review_history | practice_vocabulary |
    /// practice_quiz | none. Drives the one-click action button in the UI.
    /// </summary>
    public string ActionType { get; set; } = string.Empty;

    /// <summary>Relative app route for the step (e.g. /history, /write?focus=grammar).</summary>
    public string ActionTarget { get; set; } = string.Empty;
}

public class StudyGuideTopicDto
{
    public string Title { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string SuggestedPrompt { get; set; } = string.Empty;
    public List<string> IdeaHints { get; set; } = new();
    public List<string> KeyVocabulary { get; set; } = new();
}

/// <summary>
/// Compact summary of one graded essay (criterion scores only, never the essay
/// body) so the LLM prompt stays well under the context window even for users
/// with a long writing history.
/// </summary>
public class GradedEssaySummaryDto
{
    public Guid SubmissionId { get; set; }
    public decimal OverallBand { get; set; }
    public DateTime CreatedAt { get; set; }
    public Dictionary<string, decimal> CriteriaScores { get; set; } = new();
}

/// <summary>Written-topic signal pulled from the Writing service.</summary>
public class EssayTopicDto
{
    public Guid SubmissionId { get; set; }
    public string Title { get; set; } = string.Empty;
}

public class GrammarAggregateDto
{
    public string Category { get; set; } = string.Empty;
    public int Count { get; set; }
    public List<string> Sentences { get; set; } = new();
}

public class VocabSnapshotDto
{
    public string OriginalWord { get; set; } = string.Empty;
    public string SuggestedWord { get; set; } = string.Empty;
    public bool IsMastered { get; set; }
}

public class QuizStatsDto
{
    public int AttemptCount { get; set; }
    public decimal AverageAccuracy { get; set; }
}

/// <summary>
/// Neutral source material handed to study-guide providers (AI or deterministic
/// fallback) without leaking persistence concerns. Mirrors QuizGenerationRequest.
/// </summary>
public class StudyGuideGenerationRequest
{
    public string TargetExam { get; set; } = "IELTS Academic - Writing Task 2";
    public decimal? TargetBand { get; set; }
    public List<GradedEssaySummaryDto> EssaySummaries { get; set; } = new();
    public List<EssayTopicDto> Topics { get; set; } = new();
    public List<GrammarAggregateDto> GrammarAggregates { get; set; } = new();
    public List<VocabSnapshotDto> Vocabulary { get; set; } = new();
    public QuizStatsDto? QuizStats { get; set; }
}

/// <summary>Structured guide produced by a provider (AI or fallback).</summary>
public class StudyGuideResult
{
    public string Summary { get; set; } = string.Empty;
    public decimal EstimatedBand { get; set; }
    public List<StudyGuideInsight> Strengths { get; set; } = new();
    public List<StudyGuideInsight> Weaknesses { get; set; } = new();
    public List<StudyGuideStep> NextSteps { get; set; } = new();
    public StudyGuideTopic RecommendedTopic { get; set; } = new();
}