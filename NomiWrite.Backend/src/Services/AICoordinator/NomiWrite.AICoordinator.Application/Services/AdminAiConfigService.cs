using FluentValidation;
using Microsoft.EntityFrameworkCore;
using NomiWrite.AICoordinator.Application.DTOs;
using NomiWrite.AICoordinator.Application.Exceptions;
using NomiWrite.AICoordinator.Application.Interfaces;
using NomiWrite.AICoordinator.Domain.Entities;

namespace NomiWrite.AICoordinator.Application.Services;

public class AdminAiConfigService : IAdminAiConfigService
{
    private readonly IGradingDbContext _dbContext;
    private readonly IAiGradingProvider _gradingProvider;
    private readonly IValidator<UpdateAiGradingConfigRequestDto> _updateConfigValidator;

    public AdminAiConfigService(
        IGradingDbContext dbContext,
        IAiGradingProvider gradingProvider,
        IValidator<UpdateAiGradingConfigRequestDto> updateConfigValidator)
    {
        _dbContext = dbContext;
        _gradingProvider = gradingProvider;
        _updateConfigValidator = updateConfigValidator;
    }

    public async Task<AiGradingConfigDto> GetActiveConfigAsync()
    {
        var config = await _dbContext.AiGradingConfigs
            .AsNoTracking()
            .Where(c => c.IsActive)
            .OrderByDescending(c => c.UpdatedAt)
            .Select(c => new AiGradingConfigDto
            {
                Id = c.Id,
                ProviderName = c.ProviderName,
                ModelName = c.ModelName,
                Temperature = c.Temperature,
                SystemPromptTemplate = c.SystemPromptTemplate,
                MaxOutputTokens = c.MaxOutputTokens,
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            })
            .FirstOrDefaultAsync()
            ?? throw new AiGradingConfigNotFoundException();

        return config;
    }

    /// <summary>
    /// Creates a new active config row and deactivates all previous rows,
    /// preserving an audit trail of configuration history.
    /// </summary>
    public async Task<AiGradingConfigDto> UpdateActiveConfigAsync(UpdateAiGradingConfigRequestDto request)
    {
        var validationResult = await _updateConfigValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        // Deactivate all currently active configs.
        var activeConfigs = await _dbContext.AiGradingConfigs
            .Where(c => c.IsActive)
            .ToListAsync();

        foreach (var c in activeConfigs)
            c.IsActive = false;

        // Insert a new active config row.
        var newConfig = new AiGradingConfig
        {
            ProviderName = request.ProviderName.Trim(),
            ModelName = request.ModelName.Trim(),
            Temperature = request.Temperature,
            SystemPromptTemplate = request.SystemPromptTemplate?.Trim(),
            MaxOutputTokens = request.MaxOutputTokens,
            IsActive = true
        };

        _dbContext.AiGradingConfigs.Add(newConfig);
        await _dbContext.SaveChangesAsync();

        _gradingProvider.InvalidateActiveConfigCache();

        return new AiGradingConfigDto
        {
            Id = newConfig.Id,
            ProviderName = newConfig.ProviderName,
            ModelName = newConfig.ModelName,
            Temperature = newConfig.Temperature,
            SystemPromptTemplate = newConfig.SystemPromptTemplate,
            MaxOutputTokens = newConfig.MaxOutputTokens,
            IsActive = newConfig.IsActive,
            CreatedAt = newConfig.CreatedAt,
            UpdatedAt = newConfig.UpdatedAt
        };
    }
}