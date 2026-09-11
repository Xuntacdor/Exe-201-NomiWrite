using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NomiWrite.AICoordinator.Application.DTOs;
using NomiWrite.AICoordinator.Application.Interfaces;

namespace NomiWrite.AICoordinator.API.Controllers;

[ApiController]
[Route("api/admin/ai-config")]
[Authorize(Roles = "Admin")]
public class AdminAiConfigController : ControllerBase
{
    private readonly IAdminAiConfigService _adminAiConfigService;

    public AdminAiConfigController(IAdminAiConfigService adminAiConfigService)
        => _adminAiConfigService = adminAiConfigService;

    [HttpGet]
    public async Task<ActionResult<AiGradingConfigDto>> GetActiveConfig()
    {
        var result = await _adminAiConfigService.GetActiveConfigAsync();
        return Ok(result);
    }

    [HttpPut]
    public async Task<ActionResult<AiGradingConfigDto>> UpdateActiveConfig([FromBody] UpdateAiGradingConfigRequestDto request)
    {
        var result = await _adminAiConfigService.UpdateActiveConfigAsync(request);
        return Ok(result);
    }
}