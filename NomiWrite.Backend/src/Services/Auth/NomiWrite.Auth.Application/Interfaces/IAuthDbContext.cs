using Microsoft.EntityFrameworkCore;
using NomiWrite.Auth.Domain.Entities;

namespace NomiWrite.Auth.Application.Interfaces;

public interface IAuthDbContext
{
    DbSet<User> Users { get; }
    DbSet<RefreshToken> RefreshTokens { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}