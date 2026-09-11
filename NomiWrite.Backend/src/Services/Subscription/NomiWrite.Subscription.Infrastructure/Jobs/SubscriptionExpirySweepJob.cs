using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NomiWrite.Shared.Contracts.Events.Subscription;
using NomiWrite.Subscription.Domain.Enums;
using NomiWrite.Subscription.Infrastructure.Persistence;

namespace NomiWrite.Subscription.Infrastructure.Jobs;

public class SubscriptionExpirySweepJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SubscriptionExpirySweepJob> _logger;
    private static readonly TimeSpan Interval = TimeSpan.FromHours(1);

    public SubscriptionExpirySweepJob(
        IServiceScopeFactory scopeFactory,
        ILogger<SubscriptionExpirySweepJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("SubscriptionExpirySweepJob starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await SweepAsync(stoppingToken);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Error during subscription expiry sweep.");
            }

            await Task.Delay(Interval, stoppingToken);
        }

        _logger.LogInformation("SubscriptionExpirySweepJob stopping.");
    }

    private async Task SweepAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SubscriptionDbContext>();
        // Lấy IPublishEndpoint từ scope đã tạo thay vì inject vào Singleton
        var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

        var now = DateTime.UtcNow;
        var expiryThreshold = now.AddDays(3);

        var expiringSubscriptions = await dbContext.UserSubscriptions
            .Where(s =>
                s.Status == SubscriptionStatus.Active
                && s.EndDate >= now
                && s.EndDate <= expiryThreshold
                && !s.ExpiryWarningsSent)
            .ToListAsync(cancellationToken);

        foreach (var subscription in expiringSubscriptions)
        {
            _logger.LogInformation(
                "Publishing SubscriptionExpiringEvent for user {UserId}, plan {PlanId}, endDate {EndDate}.",
                subscription.UserId,
                subscription.PlanId,
                subscription.EndDate);

            await publishEndpoint.Publish(new SubscriptionExpiringEvent(
                subscription.UserId,
                subscription.PlanId,
                subscription.EndDate), cancellationToken);

            subscription.ExpiryWarningsSent = true;
            subscription.UpdatedAt = now;
        }

        var expiredSubscriptions = await dbContext.UserSubscriptions
            .Where(s =>
                s.Status == SubscriptionStatus.Active
                && s.EndDate <= now)
            .ToListAsync(cancellationToken);

        foreach (var subscription in expiredSubscriptions)
        {
            _logger.LogInformation(
                "Publishing SubscriptionExpiredEvent for user {UserId}, plan {PlanId}.",
                subscription.UserId,
                subscription.PlanId);

            subscription.Status = SubscriptionStatus.Expired;
            subscription.UpdatedAt = now;

            await publishEndpoint.Publish(new SubscriptionExpiredEvent(
                subscription.UserId,
                subscription.PlanId,
                now), cancellationToken);
        }

        if (expiringSubscriptions.Count > 0 || expiredSubscriptions.Count > 0)
        {
            await dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Subscription expiry sweep completed: {ExpiringCount} expiring warnings sent, {ExpiredCount} expired.",
                expiringSubscriptions.Count,
                expiredSubscriptions.Count);
        }
    }
}