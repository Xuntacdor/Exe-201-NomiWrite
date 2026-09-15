using Microsoft.EntityFrameworkCore;
using NomiWrite.Admin.Domain.Entities;

namespace NomiWrite.Admin.Application.Interfaces;

public interface IAdminDbContext
{
    DbSet<ContentReport> ContentReports { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
