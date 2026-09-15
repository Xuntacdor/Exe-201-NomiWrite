using NomiWrite.Auth.Domain.Enums;

namespace NomiWrite.Auth.Application.DTOs;

public class UpdateUserStatusRequestDto
{
    public AccountStatus Status { get; set; }
}

public class UpdateUserRoleRequestDto
{
    public UserRole Role { get; set; }
}

public class AdminUserListItemDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public AccountStatus AccountStatus { get; set; }
    public bool IsEmailVerified { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class PagedResultDto<T>
{
    public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}