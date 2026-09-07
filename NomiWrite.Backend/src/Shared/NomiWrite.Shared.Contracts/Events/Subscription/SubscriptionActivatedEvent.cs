namespace NomiWrite.Shared.Contracts.Events.Subscription;

public sealed record SubscriptionActivatedEvent(Guid UserId, Guid PlanId, DateTime EndDate);
