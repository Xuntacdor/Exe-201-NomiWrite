using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NomiWrite.User.Application.DTOs;
using NomiWrite.User.Application.Interfaces;

namespace NomiWrite.User.API.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IUserProfileService _userProfileService;

    public UserController(IUserProfileService userProfileService)
    {
        _userProfileService = userProfileService;
    }

    [HttpGet("me")]
    public async Task<ActionResult<UserProfileDto>> GetMyProfile()
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var profile = await _userProfileService.GetProfileAsync(userId.Value);
        return Ok(profile);
    }

    [HttpPut("me")]
    public async Task<ActionResult<UserProfileDto>> UpdateMyProfile([FromBody] UpdateProfileRequestDto request)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var profile = await _userProfileService.UpdateProfileAsync(userId.Value, request);
        return Ok(profile);
    }

    [HttpGet("me/account")]
    public async Task<ActionResult<MyAccountDto>> GetMyAccount()
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var myAccount = await _userProfileService.GetMyAccountAsync(userId.Value, GetBearerToken());
        return Ok(myAccount);
    }

    [HttpGet("me/progress")]
    public async Task<ActionResult<ProgressResponseDto>> GetMyProgress()
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var progress = await _userProfileService.GetProgressAsync(userId.Value, GetBearerToken());
        return Ok(progress);
    }

    private Guid? GetUserId()
    {
        var userIdValue = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        return Guid.TryParse(userIdValue, out var userId) ? userId : null;
    }

    private string? GetBearerToken()
    {
        var authorizationHeader = Request.Headers.Authorization.ToString();

        if (string.IsNullOrWhiteSpace(authorizationHeader))
            return null;

        const string bearerPrefix = "Bearer ";
        return authorizationHeader.StartsWith(bearerPrefix, StringComparison.OrdinalIgnoreCase)
            ? authorizationHeader[bearerPrefix.Length..]
            : authorizationHeader;
    }
}