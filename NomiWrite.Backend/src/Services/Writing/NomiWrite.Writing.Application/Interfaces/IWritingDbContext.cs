using Microsoft.EntityFrameworkCore;
using NomiWrite.Writing.Domain.Entities;

namespace NomiWrite.Writing.Application.Interfaces;

public interface IWritingDbContext
{
    DbSet<WritingType> WritingTypes { get; }
    DbSet<WritingPrompt> WritingPrompts { get; }
    DbSet<WritingSubmission> WritingSubmissions { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}