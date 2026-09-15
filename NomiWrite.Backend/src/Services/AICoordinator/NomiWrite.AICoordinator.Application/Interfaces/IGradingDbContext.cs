using Microsoft.EntityFrameworkCore;
using NomiWrite.AICoordinator.Domain.Entities;

namespace NomiWrite.AICoordinator.Application.Interfaces;

public interface IGradingDbContext
{
    DbSet<GradingResult> GradingResults { get; }
    DbSet<TutorReviewRequest> TutorReviewRequests { get; }
    DbSet<GradingFeedbackFlag> GradingFeedbackFlags { get; }
    DbSet<AiGradingConfig> AiGradingConfigs { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
