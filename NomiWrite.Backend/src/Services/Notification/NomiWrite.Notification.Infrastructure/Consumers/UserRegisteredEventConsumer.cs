using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NomiWrite.Notification.Application.Interfaces;
using NomiWrite.Notification.Domain.Entities;
using NomiWrite.Shared.Contracts.Events.Auth;

namespace NomiWrite.Notification.Infrastructure.Consumers;

public class UserRegisteredEventConsumer : IConsumer<UserRegisteredEvent>
{
    private readonly INotificationDbContext _dbContext;
    private readonly ILogger<UserRegisteredEventConsumer> _logger;

    public UserRegisteredEventConsumer(
        INotificationDbContext dbContext,
        ILogger<UserRegisteredEventConsumer> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<UserRegisteredEvent> context)
    {
        var @event = context.Message;

        _logger.LogInformation(
            "Creating notification preferences for registered user {UserId}.",
            @event.Id);

        var existing = await _dbContext.NotificationPreferences
            .FirstOrDefaultAsync(p => p.UserId == @event.Id);

        if (existing is not null)
        {
            _logger.LogDebug(
                "Notification preferences already exist for user {UserId}; skipping.",
                @event.Id);
            return;
        }

        var now = DateTime.UtcNow;
        _dbContext.NotificationPreferences.Add(new NotificationPreference
        {
            UserId = @event.Id,
            EmailNotificationsEnabled = true,
            InAppNotificationsEnabled = true,
            GradingAlerts = true,
            MarketingAlerts = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        await _dbContext.SaveChangesAsync();
    }
}
