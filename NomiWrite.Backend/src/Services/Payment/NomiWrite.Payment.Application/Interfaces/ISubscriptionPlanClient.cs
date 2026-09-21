namespace NomiWrite.Payment.Application.Interfaces;

public interface ISubscriptionPlanClient
{
    Task<SubscriptionPlanPrice?> GetActivePlanAsync(Guid planId);
}

public record SubscriptionPlanPrice(decimal Price, string Currency);
