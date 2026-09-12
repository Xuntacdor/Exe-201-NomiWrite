namespace NomiWrite.Shared.Contracts.Events.Grading;

public sealed record GradingCompletedEvent(
    Guid SubmissionId,
    Guid UserId,
    decimal OverallBand,
    DateTime CompletedAt,
    List<GradingGrammarErrorEventItem>? GrammarErrors = null,
    List<GradingVocabularySuggestionEventItem>? VocabularySuggestions = null);

/// <summary>
/// Grammar feedback produced by the AI grading cycle. Consumed by the Learning
/// service so personalized quizzes can be generated from real weak points
/// without re-invoking the grading provider.
/// </summary>
public sealed record GradingGrammarErrorEventItem(
    string OriginalText,
    string Suggestion,
    string Explanation);

/// <summary>
/// Vocabulary feedback produced by the AI grading cycle. Consumed by the
/// Learning service to build the user's personal vocabulary notebook.
/// </summary>
public sealed record GradingVocabularySuggestionEventItem(
    string OriginalWord,
    List<string> SuggestedAlternatives,
    string Context);