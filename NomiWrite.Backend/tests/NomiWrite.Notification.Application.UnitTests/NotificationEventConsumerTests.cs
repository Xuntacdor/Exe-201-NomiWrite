using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NomiWrite.Notification.Application.UnitTests.Persistence;
using NomiWrite.Notification.Domain.Entities;
using NomiWrite.Notification.Domain.Enums;
using NomiWrite.Notification.Infrastructure.Consumers;
using NomiWrite.Shared.Contracts.Events.Forum;
using NomiWrite.Shared.Contracts.Events.Subscription;

namespace NomiWrite.Notification.Application.UnitTests;

public class NotificationEventConsumerTests
{
    private static readonly Guid UserA = Guid.NewGuid();
    private static readonly Guid UserB = Guid.NewGuid();

    [Fact]
    public async Task SubscriptionExpiring_DuplicateEvent_NotPersistedAgain()
    {
        var db = TestNotificationDbContext.Create();
        var consumer = new SubscriptionExpiringEventConsumer(
            db, NullLogger<SubscriptionExpiringEventConsumer>.Instance);
        var evt = new SubscriptionExpiringEvent(UserA, Guid.NewGuid(), DateTime.UtcNow.AddDays(2));

        await Consume(consumer, evt);
        await Consume(consumer, evt);

        db.Notifications.Should().ContainSingle(n => n.Type == NotificationType.Subscription);
        db.Notifications.Single().ReferenceId.Should().Be(evt.PlanId);
        db.Notifications.Single().Message.Should().Contain(evt.EndDate.ToString("MMMM dd, yyyy"));
    }

    [Fact]
    public async Task SubscriptionExpired_InAppDisabled_Skips()
    {
        var db = TestNotificationDbContext.Create();
        db.NotificationPreferences.Add(new NotificationPreference
        {
            UserId = UserA,
            InAppNotificationsEnabled = false
        });
        db.SaveChanges();
        var consumer = new SubscriptionExpiredEventConsumer(
            db, NullLogger<SubscriptionExpiredEventConsumer>.Instance);

        await Consume(consumer, new SubscriptionExpiredEvent(UserA, Guid.NewGuid(), DateTime.UtcNow));

        db.Notifications.Should().BeEmpty();
    }

    [Fact]
    public async Task SubscriptionExpired_DuplicateEvent_NotPersistedAgain()
    {
        var db = TestNotificationDbContext.Create();
        var consumer = new SubscriptionExpiredEventConsumer(
            db, NullLogger<SubscriptionExpiredEventConsumer>.Instance);
        var evt = new SubscriptionExpiredEvent(UserA, Guid.NewGuid(), DateTime.UtcNow);

        await Consume(consumer, evt);
        await Consume(consumer, evt);

        db.Notifications.Should().ContainSingle(n => n.Type == NotificationType.Subscription);
        db.Notifications.Single().ReferenceId.Should().Be(evt.PlanId);
        db.Notifications.Single().Message.Should().Contain(evt.ExpiredAt.ToString("MMMM dd, yyyy"));
    }

    [Fact]
    public async Task ForumComment_SelfComment_Skips()
    {
        var db = TestNotificationDbContext.Create();
        var consumer = new ForumCommentCreatedEventConsumer(
            db, NullLogger<ForumCommentCreatedEventConsumer>.Instance);
        var evt = new ForumCommentCreatedEvent(
            Guid.NewGuid(), Guid.NewGuid(), UserA, UserA, "Self reply");

        await Consume(consumer, evt);

        db.Notifications.Should().BeEmpty();
    }

    [Fact]
    public async Task ForumComment_DuplicateEvent_NotPersistedAgain()
    {
        var db = TestNotificationDbContext.Create();
        var consumer = new ForumCommentCreatedEventConsumer(
            db, NullLogger<ForumCommentCreatedEventConsumer>.Instance);
        var evt = new ForumCommentCreatedEvent(
            Guid.NewGuid(), Guid.NewGuid(), UserA, UserB, "Great post");

        await Consume(consumer, evt);
        await Consume(consumer, evt);

        db.Notifications.Should().ContainSingle(n => n.Type == NotificationType.Forum);
        db.Notifications.Single().UserId.Should().Be(UserA);
        db.Notifications.Single().ReferenceId.Should().Be(evt.CommentId);
    }

    [Fact]
    public async Task ForumComment_InAppDisabled_Skips()
    {
        var db = TestNotificationDbContext.Create();
        db.NotificationPreferences.Add(new NotificationPreference
        {
            UserId = UserA,
            InAppNotificationsEnabled = false
        });
        db.SaveChanges();
        var consumer = new ForumCommentCreatedEventConsumer(
            db, NullLogger<ForumCommentCreatedEventConsumer>.Instance);

        await Consume(consumer, new ForumCommentCreatedEvent(
            Guid.NewGuid(), Guid.NewGuid(), UserA, UserB, "Great post"));

        db.Notifications.Should().BeEmpty();
    }

    [Fact]
    public async Task PostLiked_DuplicateEvent_NotPersistedAgain()
    {
        var db = TestNotificationDbContext.Create();
        var consumer = new PostLikedEventConsumer(
            db, NullLogger<PostLikedEventConsumer>.Instance);
        var evt = new PostLikedEvent(Guid.NewGuid(), UserA, UserB);

        await Consume(consumer, evt);
        await Consume(consumer, evt);

        db.Notifications.Should().ContainSingle(n => n.Type == NotificationType.Forum);
        db.Notifications.Single().UserId.Should().Be(UserA);
        db.Notifications.Single().ReferenceId.Should().NotBeNull();
    }

    [Fact]
    public async Task PostLiked_DifferentLikers_CreateSeparateNotifications()
    {
        var db = TestNotificationDbContext.Create();
        var consumer = new PostLikedEventConsumer(
            db, NullLogger<PostLikedEventConsumer>.Instance);
        var postId = Guid.NewGuid();

        await Consume(consumer, new PostLikedEvent(postId, UserA, UserB));
        await Consume(consumer, new PostLikedEvent(postId, UserA, Guid.NewGuid()));

        db.Notifications.Should().HaveCount(2);
        db.Notifications.Select(n => n.ReferenceId).Should().OnlyHaveUniqueItems();
    }

    private static async Task Consume<T>(IConsumer<T> consumer, T message) where T : class
    {
        var context = Substitute.For<ConsumeContext<T>>();
        context.Message.Returns(message);
        await consumer.Consume(context);
    }
}
