using Microsoft.EntityFrameworkCore;
using NomiWrite.Auth.Application.Interfaces;
using NomiWrite.Auth.Domain.Entities;

namespace NomiWrite.Auth.Application.UnitTests.Persistence;

public sealed class TestAuthDbContext : DbContext, IAuthDbContext
{
    public TestAuthDbContext(DbContextOptions<TestAuthDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<EmailVerificationToken> EmailVerificationTokens => Set<EmailVerificationToken>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();

    public static TestAuthDbContext Create()
    {
        var options = new DbContextOptionsBuilder<TestAuthDbContext>()
            .UseInMemoryDatabase($"test-auth-{Guid.NewGuid():N}")
            .EnableSensitiveDataLogging()
            .Options;
        return new TestAuthDbContext(options);
    }
}