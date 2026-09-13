using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NomiWrite.Notification.Application.DTOs;
using NomiWrite.Notification.Application.Services;
using NomiWrite.Notification.Application.UnitTests.Persistence;
using NomiWrite.Notification.Domain.Entities;
using NomiWrite.Notification.Domain.Enums;
using NomiWrite.Notification.Infrastructure.Consumers;
using NomiWrite.Notification.Infrastructure.Persistence;
using NomiWrite.Shared.Contracts.Events.Grading;
using NotificationEntity = NomiWrite.Notification.Domain.Entities.Notification;

namespace NomiWrite.Notification.Application.UnitTests;

public class NotificationServiceTests
{
    private static readonly Guid UserA = Guid.NewGuid();
    private static readonly Guid UserB = Guid.NewGuid();

    private static NotificationService Build(NotificationDbContext db) => new(db);

    private static void SeedNotification(NotificationDbContext db, Guid? userId, string title,
        NotificationType type = NotificationType.Forum, bool isRead = false, DateTime? createdAt = null)
    {
        db.Notifications.Add(new NotificationEntity
        {
            UserId = userId,
            Title = title,
            Message = "msg",
            Type = type,
            ReferenceId = Guid.NewGuid(),
            IsRead = isRead,
            CreatedAt = createdAt ?? DateTime.UtcNow,
            UpdatedAt = createdAt ?? DateTime.UtcNow
        });
        db.SaveChanges();
    }

    #region U-N3 — broadcasts visible to all (service surface)

    [Fact]
    public async Task GetNotifications_UserSeesOwnAndBroadcast()
    {
        var db = TestNotificationDbContext.Create();
        SeedNotification(db, UserA, "Direct");
        SeedNotification(db, null, "Broadcast");

        var sut = Build(db);
        var result = await sut.GetNotificationsAsync(UserA, null, 1, 20);

        result.TotalCount.Should().Be(2);
        result.Items.Select(i => i.Title).Should().Contain("Direct");
        result.Items.Select(i => i.Title).Should().Contain("Broadcast");
    }

    [Fact]
    public async Task GetNotifications_DoesNotSeeOtherUsersPrivateNotifications()
    {
        var db = TestNotificationDbContext.Create();
        SeedNotification(db, UserA, "Alice only");
        SeedNotification(db, UserB, "Bob only");
        SeedNotification(db, null, "Shared broadcast");

        var sut = Build(db);
        var result = await sut.GetNotificationsAsync(UserB, null, 1, 20);

        result.Items.Select(i => i.Title).Should().Equal("Shared broadcast", "Bob only");
    }

    #endregion

    #region U-N4 — paging + unread filter

    [Fact]
    public async Task GetNotifications_UnreadOnly_Filters()
    {
        var db = TestNotificationDbContext.Create();
        SeedNotification(db, UserA, "Read", isRead: true);
        SeedNotification(db, UserA, "Unread", isRead: false);
        SeedNotification(db, null, "Broadcast unread", isRead: false);

        var sut = Build(db);
        var result = await sut.GetNotificationsAsync(UserA, true, 1, 20);

        result.TotalCount.Should().Be(2);
        result.Items.Should().OnlyContain(i => !i.IsRead);
    }

    [Fact]
    public async Task GetNotifications_PagingWorks()
    {
        var db = TestNotificationDbContext.Create();
        for (var i = 0; i < 5; i++)
            SeedNotification(db, UserA, $"N{i}", createdAt: DateTime.UtcNow.AddMinutes(-i));

        var sut = Build(db);
        var page1 = await sut.GetNotificationsAsync(UserA, null, 1, 2);
        var page2 = await sut.GetNotificationsAsync(UserA, null, 2, 2);

        page1.TotalCount.Should().Be(5);
        page1.TotalPages.Should().Be(3);
        page1.Items.Should().HaveCount(2);
        page2.Items.Should().HaveCount(2);
        page1.Items.Select(i => i.Title).Should().Equal("N0", "N1");
        page2.Items.Select(i => i.Title).Should().Equal("N2", "N3");
    }

    #endregion

    #region U-N1 — grading notification dedup by ReferenceId/submission

    [Fact]
    public async Task Consume_DuplicateGradingEvent_NotPersistedAgain()
    {
        var db = TestNotificationDbContext.Create();
        var consumer = new GradingCompletedEventConsumer(db, NullLogger<GradingCompletedEventConsumer>.Instance);
        var evt = new GradingCompletedEvent(Guid.NewGuid(), UserA, 6.5m, DateTime.UtcNow, null, null);

        await Consume(consumer, evt);
        await Consume(consumer, evt);

        db.Notifications.Should().ContainSingle(n => n.Type == NotificationType.Grading);
        db.Notifications.Single().ReferenceId.Should().Be(evt.SubmissionId);
        db.Notifications.Single().UserId.Should().Be(UserA);
        db.Notifications.Single().Message.Should().Be("Your submission scored an overall band of 6.5.");
    }

    [Fact]
    public async Task Consume_SameSubmissionDifferentUser_BothNotified()
    {
        var db = TestNotificationDbContext.Create();
        var consumer = new GradingCompletedEventConsumer(db, NullLogger<GradingCompletedEventConsumer>.Instance);
        var submissionId = Guid.NewGuid();

        await Consume(consumer, new GradingCompletedEvent(submissionId, UserA, 5.0m, DateTime.UtcNow, null, null));
        await Consume(consumer, new GradingCompletedEvent(submissionId, UserB, 5.0m, DateTime.UtcNow, null, null));

        db.Notifications.Should().HaveCount(2);
    }

    #endregion

    #region U-N2 — preference gating for grading alerts

    [Fact]
    public async Task Consume_InAppDisabled_Skips()
    {
        var db = TestNotificationDbContext.Create();
        db.NotificationPreferences.Add(new NotificationPreference
        {
            UserId = UserA,
            InAppNotificationsEnabled = false,
            GradingAlerts = true
        });
        db.SaveChanges();

        var consumer = new GradingCompletedEventConsumer(db, NullLogger<GradingCompletedEventConsumer>.Instance);
        await Consume(consumer, new GradingCompletedEvent(Guid.NewGuid(), UserA, 6.0m, DateTime.UtcNow, null, null));

        db.Notifications.Should().BeEmpty();
    }

    [Fact]
    public async Task Consume_GradingAlertsDisabled_Skips()
    {
        var db = TestNotificationDbContext.Create();
        db.NotificationPreferences.Add(new NotificationPreference
        {
            UserId = UserA,
            InAppNotificationsEnabled = true,
            GradingAlerts = false
        });
        db.SaveChanges();

        var consumer = new GradingCompletedEventConsumer(db, NullLogger<GradingCompletedEventConsumer>.Instance);
        await Consume(consumer, new GradingCompletedEvent(Guid.NewGuid(), UserA, 7.0m, DateTime.UtcNow, null, null));

        db.Notifications.Should().BeEmpty();
    }

    [Fact]
    public async Task Consume_PreferencesUnset_CreatesDefaultsAndNotifies()
    {
        var db = TestNotificationDbContext.Create();
        var consumer = new GradingCompletedEventConsumer(db, NullLogger<GradingCompletedEventConsumer>.Instance);

        await Consume(consumer, new GradingCompletedEvent(Guid.NewGuid(), UserA, 7.5m, DateTime.UtcNow, null, null));

        db.NotificationPreferences.Should().ContainSingle(p => p.UserId == UserA);
        db.Notifications.Should().ContainSingle();
    }

    [Fact]
    public async Task Consume_AllEnabled_Notifies()
    {
        var db = TestNotificationDbContext.Create();
        db.NotificationPreferences.Add(new NotificationPreference
        {
            UserId = UserA,
            InAppNotificationsEnabled = true,
            GradingAlerts = true
        });
        db.SaveChanges();

        var consumer = new GradingCompletedEventConsumer(db, NullLogger<GradingCompletedEventConsumer>.Instance);
        await Consume(consumer, new GradingCompletedEvent(Guid.NewGuid(), UserA, 6.5m, DateTime.UtcNow, null, null));

        db.Notifications.Should().ContainSingle(n => n.Type == NotificationType.Grading);
    }

    #endregion

    private static async Task Consume<T>(IConsumer<T> consumer, T message) where T : class
    {
        var context = Substitute.For<ConsumeContext<T>>();
        context.Message.Returns(message);
        await consumer.Consume(context);
    }
}