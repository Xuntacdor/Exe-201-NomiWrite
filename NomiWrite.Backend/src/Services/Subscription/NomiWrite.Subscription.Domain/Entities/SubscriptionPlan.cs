using NomiWrite.Subscription.Domain.Common;
using NomiWrite.Subscription.Domain.Enums;

namespace NomiWrite.Subscription.Domain.Entities;

public class SubscriptionPlan : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Currency { get; set; } = "VND";
    public BillingCycle BillingCycle { get; set; }
    public int DurationDays { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<UserSubscription> UserSubscriptions { get; set; } = new List<UserSubscription>();
}
