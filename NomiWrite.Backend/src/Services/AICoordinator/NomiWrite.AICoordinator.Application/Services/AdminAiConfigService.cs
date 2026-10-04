using FluentValidation;
using System.Security.Cryptography;
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
    private readonly IAiSecretProtector _secretProtector;
    private readonly IValidator<UpdateAiGradingConfigRequestDto> _updateConfigValidator;

    public AdminAiConfigService(
        IGradingDbContext dbContext,
        IAiGradingProvider gradingProvider,
        IAiSecretProtector secretProtector,
        IValidator<UpdateAiGradingConfigRequestDto> updateConfigValidator)
    {
        _dbContext = dbContext;
        _gradingProvider = gradingProvider;
        _secretProtector = secretProtector;
        _updateConfigValidator = updateConfigValidator;
    }

    public async Task<AiGradingConfigDto> GetActiveConfigAsync()
    {
        var config = await _dbContext.AiGradingConfigs
            .AsNoTracking()
            .Where(c => c.IsActive)
            .OrderByDescending(c => c.UpdatedAt)
            .FirstOrDefaultAsync()
            ?? throw new AiGradingConfigNotFoundException();

        return ToDto(config);
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

        // Keep the current encrypted key unless the admin explicitly replaces
        // or clears it. The plaintext key is never loaded for this copy path.
        var activeConfigs = await _dbContext.AiGradingConfigs
            .Where(c => c.IsActive)
            .OrderByDescending(c => c.UpdatedAt)
            .ToListAsync();

        var apiKeyCiphertext = activeConfigs.FirstOrDefault()?.ApiKeyCiphertext;
        if (request.ClearApiKey)
            apiKeyCiphertext = null;
        else if (!string.IsNullOrWhiteSpace(request.ApiKey))
            apiKeyCiphertext = _secretProtector.Protect(request.ApiKey.Trim());

        foreach (var c in activeConfigs)
            c.IsActive = false;

        // Insert a new active config row.
        var newConfig = new AiGradingConfig
        {
            ProviderName = request.ProviderName.Trim(),
            ModelName = request.ModelName.Trim(),
            FallbackModelName = string.IsNullOrWhiteSpace(request.FallbackModelName)
                ? null
                : request.FallbackModelName.Trim(),
            ApiKeyCiphertext = apiKeyCiphertext,
            Temperature = request.Temperature,
            SystemPromptTemplate = request.SystemPromptTemplate?.Trim(),
            MaxOutputTokens = request.MaxOutputTokens,
            IsActive = true
        };

        _dbContext.AiGradingConfigs.Add(newConfig);
        await _dbContext.SaveChangesAsync();

        _gradingProvider.InvalidateActiveConfigCache();

        return ToDto(newConfig);
    }

    private AiGradingConfigDto ToDto(AiGradingConfig config)
    {
        string? apiKeyHint = null;
        if (!string.IsNullOrWhiteSpace(config.ApiKeyCiphertext) && _secretProtector.IsConfigured)
        {
            try
            {
                var plaintext = _secretProtector.Unprotect(config.ApiKeyCiphertext);
                apiKeyHint = plaintext.Length <= 4 ? "••••" : $"••••{plaintext[^4..]}";
            }
            catch (Exception exception) when (exception is CryptographicException or InvalidOperationException)
            {
                // Keep the admin recovery screen usable after a master-key
                // rotation or configuration mistake. The plaintext is never exposed.
                apiKeyHint = "Configured (replace required)";
            }
        }

        return new AiGradingConfigDto
        {
            Id = config.Id,
            ProviderName = config.ProviderName,
            ModelName = config.ModelName,
            FallbackModelName = config.FallbackModelName,
            HasStoredApiKey = !string.IsNullOrWhiteSpace(config.ApiKeyCiphertext),
            ApiKeyHint = apiKeyHint,
            Temperature = config.Temperature,
            SystemPromptTemplate = config.SystemPromptTemplate,
            MaxOutputTokens = config.MaxOutputTokens,
            IsActive = config.IsActive,
            CreatedAt = config.CreatedAt,
            UpdatedAt = config.UpdatedAt
        };
    }
}
