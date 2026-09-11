using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NomiWrite.Notification.Application.Interfaces;
using NomiWrite.Notification.Domain.Entities;
using NomiWrite.Notification.Domain.Enums;
using NomiWrite.Shared.Contracts.Events.Forum;

namespace NomiWrite.Notification.Infrastructure.Consumers;

public class PostLikedEventConsumer : IConsumer<PostLikedEvent>
{
    private readonly INotificationDbContext _dbContext;
    private readonly ILogger<PostLikedEventConsumer> _logger;

    public PostLikedEventConsumer(
        INotificationDbContext dbContext,
        ILogger<PostLikedEventConsumer> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PostLikedEvent> context)
    {
        var @event = context.Message;

        _logger.LogInformation(
            "Received PostLikedEvent for post {PostId} by liker {LikerUserId}.",
            @event.PostId,
            @event.LikerUserId);

        if (@event.PostAuthorUserId == @event.LikerUserId)
        {
            _logger.LogDebug(
                "Post author and liker are the same user {UserId}; skipping self-notification.",
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
            Title = "Someone liked your post",
            Message = "Someone liked your post.",
            Type = NotificationType.Forum,
            ReferenceId = @event.PostId,
            IsRead = false,
            CreatedAt = now,
            UpdatedAt = now
        });

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation(
            "Created post like notification for user {UserId}, post {PostId}.",
            @event.PostAuthorUserId,
            @event.PostId);
    }
}
