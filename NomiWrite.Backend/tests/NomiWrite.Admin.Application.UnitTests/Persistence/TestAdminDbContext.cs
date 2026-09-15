using Microsoft.EntityFrameworkCore;
using NomiWrite.Admin.Application.Interfaces;
using NomiWrite.Admin.Domain.Common;
using NomiWrite.Admin.Domain.Entities;

namespace NomiWrite.Admin.Application.UnitTests.Persistence;

public class TestAdminDbContext : DbContext, IAdminDbContext
{
    public TestAdminDbContext(DbContextOptions<TestAdminDbContext> options) : base(options) { }

    public DbSet<ContentReport> ContentReports => Set<ContentReport>();

    public static TestAdminDbContext Create()
    {
        var options = new DbContextOptionsBuilder<TestAdminDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new TestAdminDbContext(options);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
                entry.Entity.CreatedAt = now;
        }
        return base.SaveChangesAsync(cancellationToken);
    }
}