using NomiWrite.AICoordinator.Domain.Common;

namespace NomiWrite.AICoordinator.Domain.Entities;

public class GradingFeedbackFlag : BaseEntity
{
    public Guid GradingResultId { get; set; }
    public Guid UserId { get; set; }
    public string Reason { get; set; } = string.Empty;
}