using NomiWrite.Subscription.Domain.Enums;

namespace NomiWrite.Subscription.Application.DTOs;

public class SubscriptionPlanDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Currency { get; set; } = "VND";
    public BillingCycle BillingCycle { get; set; }
    public int DurationDays { get; set; }
}

public class UserSubscriptionStatusDto
{
    public string PlanName { get; set; } = string.Empty;
    public SubscriptionStatus Status { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int DaysRemaining { get; set; }
}
