using NomiWrite.Subscription.Application.DTOs;

namespace NomiWrite.Subscription.Application.Interfaces;

public interface IAdminSubscriptionService
{
    Task<SubscriptionAnalyticsDto> GetAnalyticsAsync();
}