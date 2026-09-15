using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NomiWrite.Auth.Application.DTOs;
using NomiWrite.Auth.Application.Interfaces;
using NomiWrite.Auth.Domain.Enums;

namespace NomiWrite.Auth.API.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminUserService _adminUserService;

    public AdminController(IAdminUserService adminUserService) => _adminUserService = adminUserService;

    [HttpGet("users")]
    public async Task<ActionResult<PagedResultDto<AdminUserListItemDto>>> GetUsers(
        [FromQuery] string? search,
        [FromQuery] UserRole? role,
        [FromQuery] AccountStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _adminUserService.GetUsersAsync(search, role, status, page, pageSize);
        return Ok(result);
    }

    [HttpGet("users/analytics")]
    public async Task<ActionResult<UserAnalyticsDto>> GetUserAnalytics()
    {
        var result = await _adminUserService.GetUserAnalyticsAsync();
        return Ok(result);
    }

    [HttpPatch("users/{id:guid}/status")]
    public async Task<ActionResult<AuthResultDto>> UpdateUserStatus(Guid id, [FromBody] UpdateUserStatusRequestDto request)
    {
        var result = await _adminUserService.UpdateUserStatusAsync(id, request);
        return Ok(result);
    }

    [HttpPatch("users/{id:guid}/role")]
    public async Task<ActionResult<AuthResultDto>> UpdateUserRole(Guid id, [FromBody] UpdateUserRoleRequestDto request)
    {
        var callerUserIdValue = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (string.IsNullOrWhiteSpace(callerUserIdValue) || !Guid.TryParse(callerUserIdValue, out var callerUserId))
            return Unauthorized();

        var result = await _adminUserService.UpdateUserRoleAsync(callerUserId, id, request);
        return Ok(result);
    }
}