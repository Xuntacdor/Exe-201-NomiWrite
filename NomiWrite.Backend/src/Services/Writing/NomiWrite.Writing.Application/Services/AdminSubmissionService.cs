using Microsoft.EntityFrameworkCore;
using NomiWrite.Writing.Application.DTOs;
using NomiWrite.Writing.Application.Interfaces;
using NomiWrite.Writing.Domain.Enums;

namespace NomiWrite.Writing.Application.Services;

public class AdminSubmissionService : IAdminSubmissionService
{
    private readonly IWritingDbContext _dbContext;

    public AdminSubmissionService(IWritingDbContext dbContext)
        => _dbContext = dbContext;

    public async Task<SubmissionAnalyticsDto> GetAnalyticsAsync()
    {
        var submissions = _dbContext.WritingSubmissions.AsNoTracking();

        return new SubmissionAnalyticsDto
        {
            TotalSubmissions = await submissions.CountAsync(),
            GradedSubmissions = await submissions.CountAsync(s => s.GradedAt.HasValue),
            PendingSubmissions = await submissions.CountAsync(s =>
                s.Status == SubmissionStatus.Submitted && !s.GradedAt.HasValue)
        };
    }
}