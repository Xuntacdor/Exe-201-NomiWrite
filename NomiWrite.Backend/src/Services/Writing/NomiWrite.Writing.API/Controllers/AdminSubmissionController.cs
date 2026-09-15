using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NomiWrite.Writing.Application.DTOs;
using NomiWrite.Writing.Application.Interfaces;

namespace NomiWrite.Writing.API.Controllers;

[ApiController]
[Route("api/admin/submissions")]
[Authorize(Roles = "Admin")]
public class AdminSubmissionController : ControllerBase
{
    private readonly IAdminSubmissionService _adminSubmissionService;

    public AdminSubmissionController(IAdminSubmissionService adminSubmissionService)
        => _adminSubmissionService = adminSubmissionService;

    [HttpGet("analytics")]
    public async Task<ActionResult<SubmissionAnalyticsDto>> GetAnalytics()
    {
        var result = await _adminSubmissionService.GetAnalyticsAsync();
        return Ok(result);
    }
}