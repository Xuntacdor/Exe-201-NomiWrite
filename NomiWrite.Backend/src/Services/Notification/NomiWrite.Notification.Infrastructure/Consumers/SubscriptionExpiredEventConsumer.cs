using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NomiWrite.Notification.Application.Interfaces;
using NomiWrite.Notification.Domain.Entities;
using NomiWrite.Notification.Domain.Enums;
using NomiWrite.Shared.Contracts.Events.Subscription;

namespace NomiWrite.Notification.Infrastructure.Consumers;

public class SubscriptionExpiredEventConsumer : IConsumer<SubscriptionExpiredEvent>
{
    private readonly INotificationDbContext _dbContext;
    private readonly ILogger<SubscriptionExpiredEventConsumer> _logger;

    public SubscriptionExpiredEventConsumer(
        INotificationDbContext dbContext,
        ILogger<SubscriptionExpiredEventConsumer> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<SubscriptionExpiredEvent> context)
    {
        var @event = context.Message;

        _logger.LogInformation(
            "Received SubscriptionExpiredEvent for user {UserId}, plan {PlanId}.",
            @event.UserId,
            @event.PlanId);

        var message = $"Your subscription expired on {@event.ExpiredAt:MMMM dd, yyyy}. Renew to continue enjoying premium features.";
        var existing = await _dbContext.Notifications
            .FirstOrDefaultAsync(n =>
                n.Type == NotificationType.Subscription
                && n.UserId == @event.UserId
                && n.ReferenceId == @event.PlanId
                && n.Title == "Your subscription has expired"
                && n.Message == message);

        if (existing is not null)
        {
            _logger.LogDebug(
                "Subscription expired notification already exists for user {UserId}, plan {PlanId}, expiredAt {ExpiredAt}; skipping.",
                @event.UserId,
                @event.PlanId,
                @event.ExpiredAt);
            return;
        }

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
                "User {UserId} has disabled in-app notifications; skipping subscription expired notification.",
                @event.UserId);
            return;
        }

        var now = DateTime.UtcNow;
        _dbContext.Notifications.Add(new Domain.Entities.Notification
        {
            UserId = @event.UserId,
            Title = "Your subscription has expired",
            Message = message,
            Type = NotificationType.Subscription,
            ReferenceId = @event.PlanId,
            IsRead = false,
            CreatedAt = now,
            UpdatedAt = now
        });

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation(
            "Created subscription expired notification for user {UserId}.",
            @event.UserId);
    }
}
