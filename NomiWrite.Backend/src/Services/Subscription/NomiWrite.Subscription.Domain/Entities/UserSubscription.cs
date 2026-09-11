using NomiWrite.Subscription.Domain.Common;
using NomiWrite.Subscription.Domain.Enums;

namespace NomiWrite.Subscription.Domain.Entities;

public class UserSubscription : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid PlanId { get; set; }
    public Guid PaymentOrderId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Active;
    public bool ExpiryWarningsSent { get; set; }

    public SubscriptionPlan? Plan { get; set; }
}
