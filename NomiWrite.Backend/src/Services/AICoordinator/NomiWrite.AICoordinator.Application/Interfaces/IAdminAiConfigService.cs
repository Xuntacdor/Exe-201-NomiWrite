using NomiWrite.AICoordinator.Application.DTOs;

namespace NomiWrite.AICoordinator.Application.Interfaces;

public interface IAdminAiConfigService
{
    Task<AiGradingConfigDto> GetActiveConfigAsync();

    Task<AiGradingConfigDto> UpdateActiveConfigAsync(UpdateAiGradingConfigRequestDto request);
}