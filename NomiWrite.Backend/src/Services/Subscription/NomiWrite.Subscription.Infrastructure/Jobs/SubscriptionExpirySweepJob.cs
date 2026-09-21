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
    private static readonly TimeSpan Interval = TimeSpan.FromHours(1);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SubscriptionExpirySweepJob> _logger;

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

        var expiringEvents = new List<SubscriptionExpiringEvent>(expiringSubscriptions.Count);
        foreach (var subscription in expiringSubscriptions)
        {
            _logger.LogInformation(
                "Marking subscription as expiring for user {UserId}, plan {PlanId}, endDate {EndDate}.",
                subscription.UserId,
                subscription.PlanId,
                subscription.EndDate);

            expiringEvents.Add(new SubscriptionExpiringEvent(
                subscription.UserId,
                subscription.PlanId,
                subscription.EndDate));

            subscription.ExpiryWarningsSent = true;
            subscription.UpdatedAt = now;
        }

        var expiredSubscriptions = await dbContext.UserSubscriptions
            .Where(s =>
                s.Status == SubscriptionStatus.Active
                && s.EndDate <= now)
            .ToListAsync(cancellationToken);

        var expiredEvents = new List<SubscriptionExpiredEvent>(expiredSubscriptions.Count);
        foreach (var subscription in expiredSubscriptions)
        {
            _logger.LogInformation(
                "Marking subscription as expired for user {UserId}, plan {PlanId}.",
                subscription.UserId,
                subscription.PlanId);

            expiredEvents.Add(new SubscriptionExpiredEvent(
                subscription.UserId,
                subscription.PlanId,
                now));

            subscription.Status = SubscriptionStatus.Expired;
            subscription.UpdatedAt = now;
        }

        if (expiringSubscriptions.Count > 0 || expiredSubscriptions.Count > 0)
        {
            await dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Subscription expiry sweep persisted: {ExpiringCount} expiring warnings marked, {ExpiredCount} expired.",
                expiringSubscriptions.Count,
                expiredSubscriptions.Count);
        }

        foreach (var evt in expiringEvents)
        {
            _logger.LogInformation(
                "Publishing SubscriptionExpiringEvent for user {UserId}, plan {PlanId}, endDate {EndDate}.",
                evt.UserId,
                evt.PlanId,
                evt.EndDate);

            await publishEndpoint.Publish(evt, cancellationToken);
        }

        foreach (var evt in expiredEvents)
        {
            _logger.LogInformation(
                "Publishing SubscriptionExpiredEvent for user {UserId}, plan {PlanId}.",
                evt.UserId,
                evt.PlanId);

            await publishEndpoint.Publish(evt, cancellationToken);
        }
    }
}
