namespace NomiWrite.Shared.Contracts.Events.Subscription;

public sealed record SubscriptionExpiredEvent(Guid UserId, Guid PlanId, DateTime ExpiredAt);
