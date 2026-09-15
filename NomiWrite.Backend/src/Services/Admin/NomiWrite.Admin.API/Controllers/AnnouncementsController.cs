using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NomiWrite.Admin.Application.DTOs;
using NomiWrite.Admin.Application.Interfaces;

namespace NomiWrite.Admin.API.Controllers;

[ApiController]
[Route("api/admin/announcements")]
[Authorize(Roles = "Admin")]
public class AnnouncementsController : ControllerBase
{
    private readonly IAnnouncementService _announcementService;

    public AnnouncementsController(IAnnouncementService announcementService)
        => _announcementService = announcementService;

    [HttpPost]
    public async Task<IActionResult> PublishAnnouncement([FromBody] AnnouncementRequestDto request)
    {
        await _announcementService.PublishAnnouncementAsync(request);
        return Accepted();
    }
}