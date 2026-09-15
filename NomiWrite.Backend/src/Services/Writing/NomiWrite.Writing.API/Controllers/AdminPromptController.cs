using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NomiWrite.Writing.Application.DTOs;
using NomiWrite.Writing.Application.Interfaces;
using NomiWrite.Writing.Domain.Enums;

namespace NomiWrite.Writing.API.Controllers;

[ApiController]
[Route("api/admin/prompts")]
[Authorize(Roles = "Admin")]
public class AdminPromptController : ControllerBase
{
    private readonly IAdminPromptService _adminPromptService;

    public AdminPromptController(IAdminPromptService adminPromptService) => _adminPromptService = adminPromptService;

    [HttpGet]
    public async Task<ActionResult<PagedResultDto<AdminPromptListItemDto>>> GetPrompts(
        [FromQuery] Guid? typeId,
        [FromQuery] DifficultyLevel? difficulty,
        [FromQuery] bool? isActive,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _adminPromptService.GetPromptsAsync(typeId, difficulty, isActive, page, pageSize);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<AdminPromptListItemDto>> CreatePrompt([FromBody] CreatePromptRequestDto request)
    {
        var result = await _adminPromptService.CreatePromptAsync(request);
        return CreatedAtAction(nameof(GetPromptById), new { id = result.Id }, result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AdminPromptListItemDto>> GetPromptById(Guid id)
    {
        var result = await _adminPromptService.GetPromptByIdAsync(id);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<AdminPromptListItemDto>> UpdatePrompt(Guid id, [FromBody] UpdatePromptRequestDto request)
    {
        var result = await _adminPromptService.UpdatePromptAsync(id, request);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeletePrompt(Guid id)
    {
        await _adminPromptService.DeletePromptAsync(id);
        return NoContent();
    }
}