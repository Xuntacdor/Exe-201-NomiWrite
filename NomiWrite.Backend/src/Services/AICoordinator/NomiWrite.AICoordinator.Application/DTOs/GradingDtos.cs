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

public class GeminiGradingResponseSchema
{
    public decimal OverallBand { get; set; }
    public List<GeminiCriterionScoreSchema> Criteria { get; set; } = new();
    public string OverallFeedback { get; set; } = string.Empty;
    public List<GeminiGrammarErrorSchema> GrammarErrors { get; set; } = new();
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
