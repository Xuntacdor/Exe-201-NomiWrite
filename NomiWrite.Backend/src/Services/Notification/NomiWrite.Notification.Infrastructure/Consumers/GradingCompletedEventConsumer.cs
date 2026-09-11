using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NomiWrite.Notification.Application.Interfaces;
using NomiWrite.Notification.Domain.Entities;
using NomiWrite.Notification.Domain.Enums;
using NomiWrite.Shared.Contracts.Events.Grading;

namespace NomiWrite.Notification.Infrastructure.Consumers;

public class GradingCompletedEventConsumer : IConsumer<GradingCompletedEvent>
{
    private readonly INotificationDbContext _dbContext;
    private readonly ILogger<GradingCompletedEventConsumer> _logger;

    public GradingCompletedEventConsumer(
        INotificationDbContext dbContext,
        ILogger<GradingCompletedEventConsumer> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<GradingCompletedEvent> context)
    {
        var @event = context.Message;

        _logger.LogInformation(
            "Received GradingCompletedEvent for submission {SubmissionId} from user {UserId}.",
            @event.SubmissionId,
            @event.UserId);

        var existing = await _dbContext.Notifications
            .FirstOrDefaultAsync(n =>
                n.Type == NotificationType.Grading
                && n.ReferenceId == @event.SubmissionId
                && n.UserId == @event.UserId);

        if (existing is not null)
        {
            _logger.LogDebug(
                "Notification already exists for submission {SubmissionId} and user {UserId}; skipping.",
                @event.SubmissionId,
                @event.UserId);
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

            _logger.LogDebug(
                "Created default notification preferences for user {UserId}.",
                @event.UserId);
        }

        if (!preference.InAppNotificationsEnabled || !preference.GradingAlerts)
        {
            _logger.LogDebug(
                "User {UserId} has disabled in-app or grading notifications; skipping.",
                @event.UserId);
            return;
        }

        var now = DateTime.UtcNow;
        _dbContext.Notifications.Add(new Domain.Entities.Notification
        {
            UserId = @event.UserId,
            Title = "Your essay has been graded",
            Message = $"Your submission scored an overall band of {@event.OverallBand}.",
            Type = NotificationType.Grading,
            ReferenceId = @event.SubmissionId,
            IsRead = false,
            CreatedAt = now,
            UpdatedAt = now
        });

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation(
            "Created grading notification for user {UserId}, submission {SubmissionId}.",
            @event.UserId,
            @event.SubmissionId);
    }
}
