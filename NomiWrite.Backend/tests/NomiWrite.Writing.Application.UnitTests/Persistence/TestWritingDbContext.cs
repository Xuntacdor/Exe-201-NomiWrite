using Microsoft.EntityFrameworkCore;
using NomiWrite.Writing.Application.Interfaces;
using NomiWrite.Writing.Domain.Common;
using NomiWrite.Writing.Domain.Entities;

namespace NomiWrite.Writing.Application.UnitTests.Persistence;

public class TestWritingDbContext : DbContext, IWritingDbContext
{
    public TestWritingDbContext(DbContextOptions<TestWritingDbContext> options) : base(options) { }

    public DbSet<WritingType> WritingTypes => Set<WritingType>();
    public DbSet<WritingPrompt> WritingPrompts => Set<WritingPrompt>();
    public DbSet<WritingSubmission> WritingSubmissions => Set<WritingSubmission>();

    public static TestWritingDbContext Create()
    {
        var options = new DbContextOptionsBuilder<TestWritingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new TestWritingDbContext(options);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
                entry.Entity.CreatedAt = now;
            else if (entry.State == EntityState.Modified)
                entry.Entity.UpdatedAt = now;
        }
        return base.SaveChangesAsync(cancellationToken);
    }
}