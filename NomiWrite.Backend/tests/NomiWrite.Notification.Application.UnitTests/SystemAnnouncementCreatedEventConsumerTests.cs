using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NomiWrite.Notification.Application.UnitTests.Persistence;
using NomiWrite.Notification.Domain.Enums;
using NomiWrite.Notification.Infrastructure.Consumers;
using NomiWrite.Shared.Contracts.Events.Admin;

namespace NomiWrite.Notification.Application.UnitTests;

public class SystemAnnouncementCreatedEventConsumerTests
{
    private static async Task Consume(SystemAnnouncementCreatedEventConsumer consumer,
        SystemAnnouncementCreatedEvent evt)
    {
        var context = Substitute.For<ConsumeContext<SystemAnnouncementCreatedEvent>>();
        context.Message.Returns(evt);
        await consumer.Consume(context);
    }

    // U-N3 — broadcast announcements (UserId == null) visible to all, not duplicated.
    [Fact]
    public async Task Consume_Announcement_StoresSingleBroadcastRow()
    {
        var db = TestNotificationDbContext.Create();
        var consumer = new SystemAnnouncementCreatedEventConsumer(
            db, NullLogger<SystemAnnouncementCreatedEventConsumer>.Instance);
        var evt = new SystemAnnouncementCreatedEvent(
            Guid.NewGuid(), "Server maintenance", "Short downtime tonight.", DateTime.UtcNow);

        await Consume(consumer, evt);

        var stored = db.Notifications.Single();
        stored.UserId.Should().BeNull();
        stored.Type.Should().Be(NotificationType.System);
        stored.ReferenceId.Should().Be(evt.AnnouncementId);
        stored.Title.Should().Be("Server maintenance");
        stored.IsRead.Should().BeFalse();
    }

    [Fact]
    public async Task Consume_DuplicateAnnouncement_NotStoredAgain()
    {
        var db = TestNotificationDbContext.Create();
        var consumer = new SystemAnnouncementCreatedEventConsumer(
            db, NullLogger<SystemAnnouncementCreatedEventConsumer>.Instance);
        var evt = new SystemAnnouncementCreatedEvent(
            Guid.NewGuid(), "Maintenance", "Downtime.", DateTime.UtcNow);

        await Consume(consumer, evt);
        await Consume(consumer, evt);

        db.Notifications.Should().ContainSingle();
    }
}