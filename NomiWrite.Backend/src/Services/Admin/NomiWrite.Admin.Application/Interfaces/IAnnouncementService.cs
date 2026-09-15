using MassTransit;
using NomiWrite.Admin.Application.DTOs;
using NomiWrite.Shared.Contracts.Events.Admin;

namespace NomiWrite.Admin.Application.Interfaces;

public interface IAnnouncementService
{
    Task PublishAnnouncementAsync(AnnouncementRequestDto request);
}
