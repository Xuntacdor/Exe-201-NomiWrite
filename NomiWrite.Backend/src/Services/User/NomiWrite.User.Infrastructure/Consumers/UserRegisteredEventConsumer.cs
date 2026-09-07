using MassTransit;
using Microsoft.Extensions.Logging;
using NomiWrite.Shared.Contracts.Events.Auth;
using NomiWrite.User.Application.Interfaces;

namespace NomiWrite.User.Infrastructure.Consumers;

public class UserRegisteredEventConsumer : IConsumer<UserRegisteredEvent>
{
    private readonly IUserProfileService _userProfileService;
    private readonly ILogger<UserRegisteredEventConsumer> _logger;

    public UserRegisteredEventConsumer(
        IUserProfileService userProfileService,
        ILogger<UserRegisteredEventConsumer> logger)
    {
        _userProfileService = userProfileService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<UserRegisteredEvent> context)
    {
        var @event = context.Message;

        _logger.LogInformation(
            "Creating user profile for registered user {UserId}.",
            @event.Id);

        await _userProfileService.CreateProfileFromRegistrationAsync(@event.Id, @event.FullName);
    }
}