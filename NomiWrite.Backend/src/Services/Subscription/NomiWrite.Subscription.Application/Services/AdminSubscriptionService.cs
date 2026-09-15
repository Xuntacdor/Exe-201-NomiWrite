using Microsoft.EntityFrameworkCore;
using NomiWrite.Subscription.Application.DTOs;
using NomiWrite.Subscription.Application.Interfaces;
using NomiWrite.Subscription.Domain.Enums;

namespace NomiWrite.Subscription.Application.Services;

public class AdminSubscriptionService : IAdminSubscriptionService
{
    private readonly ISubscriptionDbContext _dbContext;

    public AdminSubscriptionService(ISubscriptionDbContext dbContext)
        => _dbContext = dbContext;

    public async Task<SubscriptionAnalyticsDto> GetAnalyticsAsync()
    {
        var now = DateTime.UtcNow;

        // Every paid subscription grants VIP/premium access, so "VIP members"
        // = distinct users with a currently active subscription.
        var activeVipMembers = await _dbContext.UserSubscriptions
            .AsNoTracking()
            .Where(us =>
                us.Status == SubscriptionStatus.Active &&
                us.StartDate <= now &&
                us.EndDate >= now)
            .Select(us => us.UserId)
            .Distinct()
            .CountAsync();

        return new SubscriptionAnalyticsDto { ActiveVipMembers = activeVipMembers };
    }
}