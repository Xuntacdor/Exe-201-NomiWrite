using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NomiWrite.Subscription.Application.DTOs;
using NomiWrite.Subscription.Application.Interfaces;

namespace NomiWrite.Subscription.API.Controllers;

[ApiController]
[Route("api/subscriptions")]
public class SubscriptionController : ControllerBase
{
    private readonly ISubscriptionService _subscriptionService;

    public SubscriptionController(ISubscriptionService subscriptionService)
    {
        _subscriptionService = subscriptionService;
    }

    [HttpGet("plans")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<SubscriptionPlanDto>>> GetActivePlans()
    {
        var plans = await _subscriptionService.GetActivePlansAsync();
        return Ok(plans);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserSubscriptionStatusDto>> GetCurrentSubscription()
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var status = await _subscriptionService.GetCurrentSubscriptionAsync(userId.Value);

        if (status is null)
        {
            return Ok(new
            {
                hasSubscription = false,
                status = (object?)null
            });
        }

        return Ok(new
        {
            hasSubscription = true,
            status
        });
    }

    [HttpPost("me/cancel")]
    [Authorize]
    public async Task<ActionResult<UserSubscriptionStatusDto>> CancelSubscription()
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var status = await _subscriptionService.CancelSubscriptionAsync(userId.Value);
        return Ok(status);
    }

    [HttpGet("promo-codes/{code}/validate")]
    [AllowAnonymous]
    public async Task<ActionResult<PromoCodeValidationResultDto>> ValidatePromoCode(string code)
    {
        var result = await _subscriptionService.ValidatePromoCodeAsync(code);
        return Ok(result);
    }

    private Guid? GetUserId()
    {
        var userIdValue = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        return Guid.TryParse(userIdValue, out var userId) ? userId : null;
    }
}
