using FluentValidation;
using Microsoft.Extensions.Logging;
using NomiWrite.Logging.Application.DTOs;
using NomiWrite.Logging.Application.Interfaces;

namespace NomiWrite.Logging.Application.Services;

public class ActivityLogService : IActivityLogService
{
    private readonly IActivityLogRepository _repository;
    private readonly IValidator<ActivityLogQueryDto> _queryValidator;
    private readonly ILogger<ActivityLogService> _logger;

    public ActivityLogService(
        IActivityLogRepository repository,
        IValidator<ActivityLogQueryDto> queryValidator,
        ILogger<ActivityLogService> logger)
    {
        _repository = repository;
        _queryValidator = queryValidator;
        _logger = logger;
    }

    public async Task<PagedResultDto<ActivityLogItemDto>> GetPagedAsync(
        ActivityLogQueryDto query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var validationResult = await _queryValidator.ValidateAsync(query, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        return await _repository.GetPagedAsync(query, cancellationToken);
    }
}