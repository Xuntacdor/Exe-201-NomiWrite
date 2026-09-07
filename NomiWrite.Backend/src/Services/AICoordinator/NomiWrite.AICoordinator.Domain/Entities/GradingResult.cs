using NomiWrite.AICoordinator.Domain.Common;
using NomiWrite.AICoordinator.Domain.Enums;

namespace NomiWrite.AICoordinator.Domain.Entities;

public class GradingResult : BaseEntity
{
    public Guid SubmissionId { get; set; }
    public Guid UserId { get; set; }
    public decimal OverallBand { get; set; }
    public List<CriterionScore> CriterionScores { get; set; } = new();
    public string OverallFeedback { get; set; } = string.Empty;
    public string GrammarErrorsJson { get; set; } = "[]";
    public GradingStatus Status { get; set; } = GradingStatus.Pending;
    public string? ErrorMessage { get; set; }
    public DateTime? CompletedAt { get; set; }
}
