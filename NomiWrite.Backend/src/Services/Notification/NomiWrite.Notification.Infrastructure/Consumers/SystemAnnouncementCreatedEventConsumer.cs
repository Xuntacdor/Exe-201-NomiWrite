using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NomiWrite.Notification.Application.Interfaces;
using NomiWrite.Notification.Domain.Enums;
using NomiWrite.Shared.Contracts.Events.Admin;

namespace NomiWrite.Notification.Infrastructure.Consumers;

public class SystemAnnouncementCreatedEventConsumer : IConsumer<SystemAnnouncementCreatedEvent>
{
    private readonly INotificationDbContext _dbContext;
    private readonly ILogger<SystemAnnouncementCreatedEventConsumer> _logger;

    public SystemAnnouncementCreatedEventConsumer(
        INotificationDbContext dbContext,
        ILogger<SystemAnnouncementCreatedEventConsumer> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<SystemAnnouncementCreatedEvent> context)
    {
        var @event = context.Message;

        _logger.LogInformation(
            "Received SystemAnnouncementCreatedEvent {AnnouncementId}: {Title}.",
            @event.AnnouncementId,
            @event.Title);

        // Single broadcast row shared by all users (UserId == null).
        // Do NOT fan out one row per user.
        var existing = await _dbContext.Notifications
            .FirstOrDefaultAsync(n =>
                n.Type == NotificationType.System
                && n.ReferenceId == @event.AnnouncementId);

        if (existing is not null)
        {
            _logger.LogDebug(
                "Broadcast notification for announcement {AnnouncementId} already exists; skipping.",
                @event.AnnouncementId);
            return;
        }

        _dbContext.Notifications.Add(new Domain.Entities.Notification
        {
            UserId = null,
            Title = @event.Title,
            Message = @event.Message,
            Type = NotificationType.System,
            ReferenceId = @event.AnnouncementId,
            IsRead = false,
            CreatedAt = @event.CreatedAt,
            UpdatedAt = @event.CreatedAt
        });

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation(
            "Inserted broadcast notification for announcement {AnnouncementId}.",
            @event.AnnouncementId);
    }
}