using FluentValidation;
using NomiWrite.Admin.Application.DTOs;
using NomiWrite.Admin.Application.Interfaces;
using NomiWrite.Shared.Contracts.Events.Admin;
using MassTransit;

namespace NomiWrite.Admin.Application.Services;

public class AnnouncementService : IAnnouncementService
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IValidator<AnnouncementRequestDto> _validator;

    public AnnouncementService(IPublishEndpoint publishEndpoint, IValidator<AnnouncementRequestDto> validator)
    {
        _publishEndpoint = publishEndpoint;
        _validator = validator;
    }

    public async Task PublishAnnouncementAsync(AnnouncementRequestDto request)
    {
        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var announcementId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        await _publishEndpoint.Publish(new SystemAnnouncementCreatedEvent(
            announcementId,
            request.Title.Trim(),
            request.Message.Trim(),
            now));
    }
}
