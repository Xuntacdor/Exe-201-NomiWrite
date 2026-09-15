using FluentValidation;
using Microsoft.EntityFrameworkCore;
using NomiWrite.Auth.Application.DTOs;
using NomiWrite.Auth.Application.Exceptions;
using NomiWrite.Auth.Application.Interfaces;
using NomiWrite.Auth.Domain.Enums;

namespace NomiWrite.Auth.Application.Services;

public class AdminUserService : IAdminUserService
{
    private readonly IAuthDbContext _dbContext;
    private readonly IValidator<UpdateUserStatusRequestDto> _updateStatusValidator;
    private readonly IValidator<UpdateUserRoleRequestDto> _updateRoleValidator;

    public AdminUserService(
        IAuthDbContext dbContext,
        IValidator<UpdateUserStatusRequestDto> updateStatusValidator,
        IValidator<UpdateUserRoleRequestDto> updateRoleValidator)
    {
        _dbContext = dbContext;
        _updateStatusValidator = updateStatusValidator;
        _updateRoleValidator = updateRoleValidator;
    }

    public async Task<PagedResultDto<AdminUserListItemDto>> GetUsersAsync(
        string? search, UserRole? role, AccountStatus? status, int page, int pageSize)
    {
        // IgnoreQueryFilters so deactivated (soft-deleted) accounts remain visible to admins.
        var query = _dbContext.Users
            .AsNoTracking()
            .IgnoreQueryFilters();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var keyword = search.Trim().ToLowerInvariant();
            query = query.Where(u =>
                u.Email.ToLower().Contains(keyword) ||
                u.FullName.ToLower().Contains(keyword));
        }

        if (role.HasValue)
            query = query.Where(u => u.Role == role.Value);

        if (status.HasValue)
            query = query.Where(u => u.AccountStatus == status.Value);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new AdminUserListItemDto
            {
                Id = u.Id,
                Email = u.Email,
                FullName = u.FullName,
                Role = u.Role,
                AccountStatus = u.AccountStatus,
                IsEmailVerified = u.IsEmailVerified,
                CreatedAt = u.CreatedAt
            })
            .ToListAsync();

        return new PagedResultDto<AdminUserListItemDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }

    public async Task<AuthResultDto> UpdateUserStatusAsync(Guid targetUserId, UpdateUserStatusRequestDto request)
    {
        var validationResult = await _updateStatusValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var user = await _dbContext.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == targetUserId)
            ?? throw new UserNotFoundException();

        if (user.AccountStatus == request.Status)
            return Success($"This account is already set to {request.Status} status.");

        user.AccountStatus = request.Status;

        // Banned / deactivated accounts lose every active session (same pattern as
        // password-reset and self-deactivation).
        if (request.Status == AccountStatus.Banned || request.Status == AccountStatus.Deactivated)
        {
            var activeTokens = await _dbContext.RefreshTokens
                .Where(rt => rt.UserId == targetUserId && !rt.IsRevoked)
                .ToListAsync();
            foreach (var token in activeTokens)
                token.IsRevoked = true;
        }

        await _dbContext.SaveChangesAsync();

        return Success($"User account status updated to {request.Status}.");
    }

    public async Task<AuthResultDto> UpdateUserRoleAsync(Guid callerUserId, Guid targetUserId, UpdateUserRoleRequestDto request)
    {
        var validationResult = await _updateRoleValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        // Prevent an admin from re-roleing themselves to avoid accidental lockout.
        if (callerUserId == targetUserId)
            throw new CannotModifyOwnRoleException();

        var user = await _dbContext.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == targetUserId)
            ?? throw new UserNotFoundException();

        if (user.Role == request.Role)
            return Success($"This user is already assigned the {request.Role} role.");

        user.Role = request.Role;
        await _dbContext.SaveChangesAsync();

        return Success($"User role updated to {request.Role}.");
    }

    private static AuthResultDto Success(string message)
        => new() { Success = true, Message = message };

    public async Task<UserAnalyticsDto> GetUserAnalyticsAsync()
    {
        // Hard-deleted (IsDeleted) accounts are excluded from live counts.
        var users = _dbContext.Users.AsNoTracking().Where(u => !u.IsDeleted);

        return new UserAnalyticsDto
        {
            TotalUsers = await users.CountAsync(),
            ActiveUsers = await users.CountAsync(u => u.AccountStatus == AccountStatus.Active),
            BannedUsers = await users.CountAsync(u => u.AccountStatus == AccountStatus.Banned)
        };
    }
}