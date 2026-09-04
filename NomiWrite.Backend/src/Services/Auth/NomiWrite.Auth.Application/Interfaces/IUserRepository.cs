using NomiWrite.Auth.Domain.Entities;

namespace NomiWrite.Auth.Application.Interfaces;

/// <summary>
/// User repository — data access for the Auth service's own database.
/// </summary>
public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByEmailAsync(string email);
    Task<User> CreateAsync(User user);
    Task UpdateAsync(User user);
    Task<bool> ExistsAsync(string email);
}
