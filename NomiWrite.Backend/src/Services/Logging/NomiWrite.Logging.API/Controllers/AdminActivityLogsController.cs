using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NomiWrite.Logging.Application.DTOs;
using NomiWrite.Logging.Application.Interfaces;

namespace NomiWrite.Logging.API.Controllers;

[ApiController]
[Route("api/admin/logs")]
[Authorize(Roles = "Admin")]
public class AdminActivityLogsController : ControllerBase
{
    private readonly IActivityLogService _activityLogService;

    public AdminActivityLogsController(IActivityLogService activityLogService)
        => _activityLogService = activityLogService;

    /// <summary>
    /// Returns activity logs across all users. Supports filtering by user/action/time
    /// range, sorting, and pagination.
    /// </summary>
    [HttpGet("activity")]
    public async Task<ActionResult<PagedResultDto<ActivityLogItemDto>>> GetActivityLogs(
        [FromQuery] string? userId,
        [FromQuery] string? action,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sortBy = null,
        [FromQuery] SortDirection sortDirection = SortDirection.Desc,
        CancellationToken cancellationToken = default)
    {
        var query = new ActivityLogQueryDto
        {
            UserId = userId,
            Action = action,
            FromDate = fromDate,
            ToDate = toDate,
            PageIndex = pageIndex,
            PageSize = pageSize,
            SortBy = sortBy,
            SortDirection = sortDirection
        };

        var result = await _activityLogService.GetPagedAsync(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Returns the activity timeline for a single user (newest first by default).
    /// </summary>
    [HttpGet("activity/user/{userId}")]
    public async Task<ActionResult<PagedResultDto<ActivityLogItemDto>>> GetUserActivityTimeline(
        string userId,
        [FromQuery] string? action,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new ActivityLogQueryDto
        {
            UserId = userId,
            Action = action,
            FromDate = fromDate,
            ToDate = toDate,
            PageIndex = pageIndex,
            PageSize = pageSize,
            SortBy = "timestamp",
            SortDirection = SortDirection.Desc
        };

        var result = await _activityLogService.GetPagedAsync(query, cancellationToken);
        return Ok(result);
    }
}