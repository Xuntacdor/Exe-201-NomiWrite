using NomiWrite.Auth.Application.Interfaces;
using NomiWrite.Auth.Domain.Entities;
using NomiWrite.Auth.Infrastructure.Persistence;

namespace NomiWrite.Auth.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of IUserRepository for the Auth database.
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly AuthDbContext _dbContext;

    public UserRepository(AuthDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<User?> GetByIdAsync(Guid id) => throw new NotImplementedException();
    public Task<User?> GetByEmailAsync(string email) => throw new NotImplementedException();
    public Task<User> CreateAsync(User user) => throw new NotImplementedException();
    public Task UpdateAsync(User user) => throw new NotImplementedException();
    public Task<bool> ExistsAsync(string email) => throw new NotImplementedException();
}
