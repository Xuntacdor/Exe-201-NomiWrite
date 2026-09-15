using NomiWrite.Auth.Application.DTOs;
using NomiWrite.Auth.Domain.Enums;

namespace NomiWrite.Auth.Application.Interfaces;

/// <summary>
/// Admin-gated user management — delegated from the JWT "Admin" role claim (UC71).
/// </summary>
public interface IAdminUserService
{
    Task<PagedResultDto<AdminUserListItemDto>> GetUsersAsync(
        string? search, UserRole? role, AccountStatus? status, int page, int pageSize);

    Task<AuthResultDto> UpdateUserStatusAsync(Guid targetUserId, UpdateUserStatusRequestDto request);

    Task<AuthResultDto> UpdateUserRoleAsync(Guid callerUserId, Guid targetUserId, UpdateUserRoleRequestDto request);

    Task<UserAnalyticsDto> GetUserAnalyticsAsync();
}