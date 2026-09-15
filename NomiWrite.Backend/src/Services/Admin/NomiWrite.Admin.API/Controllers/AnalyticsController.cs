using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NomiWrite.Admin.Application.DTOs;
using NomiWrite.Admin.Application.Interfaces;

namespace NomiWrite.Admin.API.Controllers;

[ApiController]
[Route("api/admin/analytics")]
[Authorize(Roles = "Admin")]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;

    public AnalyticsController(IAnalyticsService analyticsService)
        => _analyticsService = analyticsService;

    [HttpGet("overview")]
    public async Task<ActionResult<AnalyticsOverviewDto>> GetOverview()
    {
        var result = await _analyticsService.GetOverviewAsync();
        return Ok(result);
    }
}