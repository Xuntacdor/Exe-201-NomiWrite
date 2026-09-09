namespace NomiWrite.AICoordinator.Application.Interfaces;

public interface ISubscriptionStatusClient
{
    Task<SubscriptionStatusResult> GetCurrentSubscriptionAsync(Guid userId, string? accessToken);
}

public record SubscriptionStatusResult(
    bool HasActiveSubscription,
    string? PlanName,
    DateTime? EndDate);