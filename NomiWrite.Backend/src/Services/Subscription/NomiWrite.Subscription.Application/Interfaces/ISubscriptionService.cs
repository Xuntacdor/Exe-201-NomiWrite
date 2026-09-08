using NomiWrite.Subscription.Application.DTOs;

namespace NomiWrite.Subscription.Application.Interfaces;

public interface ISubscriptionService
{
    Task<IEnumerable<SubscriptionPlanDto>> GetActivePlansAsync();
    Task<UserSubscriptionStatusDto?> GetCurrentSubscriptionAsync(Guid userId);
    Task ActivateSubscriptionFromPaymentAsync(Guid userId, Guid planId, Guid paymentOrderId);
    Task<UserSubscriptionStatusDto> CancelSubscriptionAsync(Guid userId);
    Task<PromoCodeValidationResultDto> ValidatePromoCodeAsync(string code);
}
