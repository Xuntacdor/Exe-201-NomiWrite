using NomiWrite.Logging.Application.DTOs;
using NomiWrite.Logging.Domain.Entities;

namespace NomiWrite.Logging.Application.Interfaces;

/// <summary>
/// Persistence contract for activity log entries (backed by MongoDB).
/// </summary>
public interface IActivityLogRepository
{
    Task AddAsync(UserActivityLog log, CancellationToken cancellationToken = default);

    Task<PagedResultDto<ActivityLogItemDto>> GetPagedAsync(
        ActivityLogQueryDto query,
        CancellationToken cancellationToken = default);
}