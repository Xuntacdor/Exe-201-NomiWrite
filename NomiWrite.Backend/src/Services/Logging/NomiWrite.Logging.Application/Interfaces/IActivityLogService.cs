using NomiWrite.Logging.Application.DTOs;

namespace NomiWrite.Logging.Application.Interfaces;

public interface IActivityLogService
{
    Task<PagedResultDto<ActivityLogItemDto>> GetPagedAsync(
        ActivityLogQueryDto query,
        CancellationToken cancellationToken = default);
}