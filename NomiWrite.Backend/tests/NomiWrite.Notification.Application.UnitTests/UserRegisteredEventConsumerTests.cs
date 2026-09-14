using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NomiWrite.Notification.Application.UnitTests.Persistence;
using NomiWrite.Notification.Infrastructure.Consumers;
using NomiWrite.Shared.Contracts.Events.Auth;

namespace NomiWrite.Notification.Application.UnitTests;

public class UserRegisteredEventConsumerTests
{
    [Fact]
    public async Task Consume_CreatesDefaultNotificationPreferences()
    {
        var db = TestNotificationDbContext.Create();
        var userId = Guid.NewGuid();
        var consumer = new UserRegisteredEventConsumer(db, NullLogger<UserRegisteredEventConsumer>.Instance);

        await Consume(consumer, new UserRegisteredEvent(userId, "student@example.com", "Student One"));

        var preferences = db.NotificationPreferences.Single();
        preferences.UserId.Should().Be(userId);
        preferences.EmailNotificationsEnabled.Should().BeTrue();
        preferences.InAppNotificationsEnabled.Should().BeTrue();
        preferences.GradingAlerts.Should().BeTrue();
        preferences.MarketingAlerts.Should().BeTrue();
    }

    [Fact]
    public async Task Consume_DuplicateRegistrationEvent_IsIdempotent()
    {
        var db = TestNotificationDbContext.Create();
        var userId = Guid.NewGuid();
        var consumer = new UserRegisteredEventConsumer(db, NullLogger<UserRegisteredEventConsumer>.Instance);
        var message = new UserRegisteredEvent(userId, "student@example.com", "Student One");

        await Consume(consumer, message);
        await Consume(consumer, message);

        db.NotificationPreferences.Should().ContainSingle(p => p.UserId == userId);
    }

    private static async Task Consume<T>(IConsumer<T> consumer, T message) where T : class
    {
        var context = Substitute.For<ConsumeContext<T>>();
        context.Message.Returns(message);
        await consumer.Consume(context);
    }
}
