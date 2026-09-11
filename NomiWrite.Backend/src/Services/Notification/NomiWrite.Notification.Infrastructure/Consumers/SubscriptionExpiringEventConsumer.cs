using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NomiWrite.Notification.Application.Interfaces;
using NomiWrite.Notification.Domain.Entities;
using NomiWrite.Notification.Domain.Enums;
using NomiWrite.Shared.Contracts.Events.Subscription;

namespace NomiWrite.Notification.Infrastructure.Consumers;

public class SubscriptionExpiringEventConsumer : IConsumer<SubscriptionExpiringEvent>
{
    private readonly INotificationDbContext _dbContext;
    private readonly ILogger<SubscriptionExpiringEventConsumer> _logger;

    public SubscriptionExpiringEventConsumer(
        INotificationDbContext dbContext,
        ILogger<SubscriptionExpiringEventConsumer> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<SubscriptionExpiringEvent> context)
    {
        var @event = context.Message;

        _logger.LogInformation(
            "Received SubscriptionExpiringEvent for user {UserId}, plan {PlanId}.",
            @event.UserId,
            @event.PlanId);

        var preference = await _dbContext.NotificationPreferences
            .FirstOrDefaultAsync(p => p.UserId == @event.UserId);

        if (preference is null)
        {
            var nowPref = DateTime.UtcNow;
            preference = new NotificationPreference
            {
                UserId = @event.UserId,
                EmailNotificationsEnabled = true,
                InAppNotificationsEnabled = true,
                GradingAlerts = true,
                MarketingAlerts = true,
                CreatedAt = nowPref,
                UpdatedAt = nowPref
            };
            _dbContext.NotificationPreferences.Add(preference);
            await _dbContext.SaveChangesAsync();
        }

        if (!preference.InAppNotificationsEnabled)
        {
            _logger.LogDebug(
                "User {UserId} has disabled in-app notifications; skipping subscription expiry warning.",
                @event.UserId);
            return;
        }

        var now = DateTime.UtcNow;
        _dbContext.Notifications.Add(new Domain.Entities.Notification
        {
            UserId = @event.UserId,
            Title = "Your subscription is expiring soon",
            Message = $"Your subscription will expire on {@event.EndDate:MMMM dd, yyyy}.",
            Type = NotificationType.Subscription,
            ReferenceId = null,
            IsRead = false,
            CreatedAt = now,
            UpdatedAt = now
        });

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation(
            "Created subscription expiring notification for user {UserId}.",
            @event.UserId);
    }
}
