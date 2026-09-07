using Microsoft.EntityFrameworkCore;
using NomiWrite.User.Domain.Entities;

namespace NomiWrite.User.Application.Interfaces;

public interface IUserDbContext
{
    DbSet<UserProfile> UserProfiles { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
