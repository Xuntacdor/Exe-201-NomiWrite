namespace NomiWrite.Shared.Contracts.Events.Subscription;

public sealed record SubscriptionExpiringEvent(Guid UserId, Guid PlanId, DateTime EndDate);
