using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NomiWrite.Admin.Application.DTOs;
using NomiWrite.Admin.Application.Interfaces;
using NomiWrite.Admin.Domain.Enums;

namespace NomiWrite.Admin.API.Controllers;

[ApiController]
[Route("api/moderation/reports")]
public class ModerationController : ControllerBase
{
    private readonly IContentReportService _contentReportService;

    public ModerationController(IContentReportService contentReportService)
        => _contentReportService = contentReportService;

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<ContentReportDto>> CreateReport([FromBody] CreateReportRequestDto request)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await _contentReportService.CreateReportAsync(userId.Value, request);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Moderator")]
    public async Task<ActionResult<ContentReportListResponseDto>> GetReports(
        [FromQuery] ReportStatus? status,
        [FromQuery] ContentType? contentType,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _contentReportService.GetReportsAsync(status, contentType, page, pageSize);
        return Ok(result);
    }

    [HttpPatch("{id:guid}/resolve")]
    [Authorize(Roles = "Admin,Moderator")]
    public async Task<ActionResult<ContentReportDto>> ResolveReport(
        Guid id,
        [FromBody] ResolveReportRequestDto request)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await _contentReportService.ResolveReportAsync(id, userId.Value, request);
        return Ok(result);
    }

    private Guid? GetUserId()
    {
        var userIdValue = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        return Guid.TryParse(userIdValue, out var userId) ? userId : null;
    }
}