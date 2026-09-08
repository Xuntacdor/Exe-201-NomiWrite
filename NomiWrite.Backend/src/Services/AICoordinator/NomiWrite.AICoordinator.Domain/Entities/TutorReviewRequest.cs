using NomiWrite.AICoordinator.Domain.Common;
using NomiWrite.AICoordinator.Domain.Enums;

namespace NomiWrite.AICoordinator.Domain.Entities;

public class TutorReviewRequest : BaseEntity
{
    public Guid SubmissionId { get; set; }
    public Guid UserId { get; set; }
    public TutorReviewStatus Status { get; set; } = TutorReviewStatus.Pending;
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
}