using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NomiWrite.Shared.Contracts.Events.Auth;
using NomiWrite.User.Application.Interfaces;
using NomiWrite.User.Infrastructure.Consumers;

namespace NomiWrite.User.Application.UnitTests;

public class UserRegisteredEventConsumerTests
{
    private static async Task Consume(IUserProfileService service, Guid userId, string fullName)
    {
        var consumer = new UserRegisteredEventConsumer(service, NullLogger<UserRegisteredEventConsumer>.Instance);
        var context = Substitute.For<ConsumeContext<UserRegisteredEvent>>();
        context.Message.Returns(new UserRegisteredEvent(userId, "alice@example.com", fullName));
        await consumer.Consume(context);
    }

    [Fact]
    public async Task Consume_DelegatesToProfileCreation()
    {
        var service = Substitute.For<IUserProfileService>();
        var userId = Guid.NewGuid();

        await Consume(service, userId, "Alice");

        await service.Received(1).CreateProfileFromRegistrationAsync(userId, "Alice");
    }
}