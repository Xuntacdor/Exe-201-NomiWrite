using NomiWrite.AICoordinator.Domain.Enums;

namespace NomiWrite.AICoordinator.Application.DTOs;

public class GradingResultDto
{
    public Guid Id { get; set; }
    public Guid SubmissionId { get; set; }
    public decimal OverallBand { get; set; }
    public List<CriterionScoreDto> CriterionScores { get; set; } = new();
    public string OverallFeedback { get; set; } = string.Empty;
    public List<GrammarErrorDto> GrammarErrors { get; set; } = new();
    public List<VocabularySuggestionDto> VocabularySuggestions { get; set; } = new();
    public List<RestructuringSuggestionDto> RestructuringSuggestions { get; set; } = new();
    public GradingStatus Status { get; set; }
}

public class CriterionScoreDto
{
    public string CriterionName { get; set; } = string.Empty;
    public decimal Score { get; set; }
    public string Comment { get; set; } = string.Empty;
}

public class GrammarErrorDto
{
    public string OriginalText { get; set; } = string.Empty;
    public string Suggestion { get; set; } = string.Empty;
    public string Explanation { get; set; } = string.Empty;
}

public class VocabularySuggestionDto
{
    public string OriginalWord { get; set; } = string.Empty;
    public List<string> SuggestedAlternatives { get; set; } = new();
    public string Context { get; set; } = string.Empty;
}

public class RestructuringSuggestionDto
{
    public string OriginalSentence { get; set; } = string.Empty;
    public string SuggestedRewrite { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}

public class GradingHistoryItemDto
{
    public Guid Id { get; set; }
    public Guid SubmissionId { get; set; }
    public decimal OverallBand { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ComparisonDto
{
    public GradingResultDto Current { get; set; } = new();
    public GradingResultDto? Previous { get; set; }
    public decimal? BandDifference { get; set; }
}

public class TutorReviewRequestDto
{
    public Guid Id { get; set; }
    public Guid SubmissionId { get; set; }
    public TutorReviewStatus Status { get; set; }
    public DateTime RequestedAt { get; set; }
}

public class FlagGradingResultRequestDto
{
    public string Reason { get; set; } = string.Empty;
}

public class FeedbackFlagConfirmationDto
{
    public Guid Id { get; set; }
    public Guid GradingResultId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class GeminiGradingResponseSchema
{
    public decimal OverallBand { get; set; }
    public List<GeminiCriterionScoreSchema> Criteria { get; set; } = new();
    public string OverallFeedback { get; set; } = string.Empty;
    public List<GeminiGrammarErrorSchema> GrammarErrors { get; set; } = new();
    public List<GeminiVocabularySuggestionSchema> VocabularySuggestions { get; set; } = new();
    public List<GeminiRestructuringSuggestionSchema> RestructuringSuggestions { get; set; } = new();
}

public class GeminiCriterionScoreSchema
{
    public string Name { get; set; } = string.Empty;
    public decimal Score { get; set; }
    public string Comment { get; set; } = string.Empty;
}

public class GeminiGrammarErrorSchema
{
    public string OriginalText { get; set; } = string.Empty;
    public string Suggestion { get; set; } = string.Empty;
    public string Explanation { get; set; } = string.Empty;
}

public class GeminiVocabularySuggestionSchema
{
    public string OriginalWord { get; set; } = string.Empty;
    public List<string> SuggestedAlternatives { get; set; } = new();
    public string Context { get; set; } = string.Empty;
}

public class GeminiRestructuringSuggestionSchema
{
    public string OriginalSentence { get; set; } = string.Empty;
    public string SuggestedRewrite { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}