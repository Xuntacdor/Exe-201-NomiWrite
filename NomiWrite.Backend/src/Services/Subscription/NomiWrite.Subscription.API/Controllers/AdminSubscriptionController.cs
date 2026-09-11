using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NomiWrite.Subscription.Application.DTOs;
using NomiWrite.Subscription.Application.Interfaces;

namespace NomiWrite.Subscription.API.Controllers;

[ApiController]
[Route("api/admin/subscriptions")]
[Authorize(Roles = "Admin")]
public class AdminSubscriptionController : ControllerBase
{
    private readonly IAdminSubscriptionService _adminSubscriptionService;

    public AdminSubscriptionController(IAdminSubscriptionService adminSubscriptionService)
        => _adminSubscriptionService = adminSubscriptionService;

    [HttpGet("analytics")]
    public async Task<ActionResult<SubscriptionAnalyticsDto>> GetAnalytics()
    {
        var result = await _adminSubscriptionService.GetAnalyticsAsync();
        return Ok(result);
    }
}