using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NomiWrite.Notification.Application.Interfaces;
using NomiWrite.Notification.Domain.Entities;
using NomiWrite.Notification.Domain.Enums;
using NomiWrite.Shared.Contracts.Events.Forum;

namespace NomiWrite.Notification.Infrastructure.Consumers;

public class ForumCommentCreatedEventConsumer : IConsumer<ForumCommentCreatedEvent>
{
    private readonly INotificationDbContext _dbContext;
    private readonly ILogger<ForumCommentCreatedEventConsumer> _logger;

    public ForumCommentCreatedEventConsumer(
        INotificationDbContext dbContext,
        ILogger<ForumCommentCreatedEventConsumer> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ForumCommentCreatedEvent> context)
    {
        var @event = context.Message;

        _logger.LogInformation(
            "Received ForumCommentCreatedEvent for post {PostId} by commenter {CommenterUserId}.",
            @event.PostId,
            @event.CommenterUserId);

        if (@event.PostAuthorUserId == @event.CommenterUserId)
        {
            _logger.LogDebug(
                "Post author and commenter are the same user {UserId}; skipping self-notification.",
                @event.PostAuthorUserId);
            return;
        }

        var preference = await _dbContext.NotificationPreferences
            .FirstOrDefaultAsync(p => p.UserId == @event.PostAuthorUserId);

        if (preference is null)
        {
            var nowPref = DateTime.UtcNow;
            preference = new NotificationPreference
            {
                UserId = @event.PostAuthorUserId,
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
                "User {UserId} has disabled in-app notifications; skipping.",
                @event.PostAuthorUserId);
            return;
        }

        var now = DateTime.UtcNow;
        _dbContext.Notifications.Add(new Domain.Entities.Notification
        {
            UserId = @event.PostAuthorUserId,
            Title = "New comment on your post",
            Message = $"Someone commented on your post: \"{@event.CommentPreview}\"",
            Type = NotificationType.Forum,
            ReferenceId = @event.PostId,
            IsRead = false,
            CreatedAt = now,
            UpdatedAt = now
        });

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation(
            "Created forum comment notification for user {UserId}, post {PostId}.",
            @event.PostAuthorUserId,
            @event.PostId);
    }
}
