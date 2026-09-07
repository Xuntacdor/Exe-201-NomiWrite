using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NomiWrite.Writing.Application.DTOs;
using NomiWrite.Writing.Application.Interfaces;
using NomiWrite.Writing.Domain.Enums;

namespace NomiWrite.Writing.API.Controllers;

[ApiController]
[Route("api/writing")]
public class WritingController : ControllerBase
{
    private readonly IWritingService _writingService;

    public WritingController(IWritingService writingService) => _writingService = writingService;

    [HttpGet("types")]
    public async Task<ActionResult<IReadOnlyList<WritingTypeDto>>> GetWritingTypes()
    {
        var result = await _writingService.GetWritingTypesAsync();
        return Ok(result);
    }

    [HttpGet("prompts")]
    public async Task<ActionResult<IReadOnlyList<WritingPromptListItemDto>>> GetPrompts(
        [FromQuery] Guid? typeId,
        [FromQuery] DifficultyLevel? difficulty,
        [FromQuery] bool random = false)
    {
        var result = await _writingService.GetPromptsAsync(typeId, difficulty, random);
        return Ok(result);
    }

    [HttpGet("prompts/{id:guid}")]
    public async Task<ActionResult<WritingPromptDto>> GetPrompt(Guid id)
    {
        var result = await _writingService.GetPromptByIdAsync(id);
        return Ok(result);
    }

    [HttpPost("submissions")]
    [Authorize]
    public async Task<ActionResult<SubmissionResponseDto>> CreateSubmission([FromBody] CreateSubmissionRequestDto request)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await _writingService.CreateSubmissionAsync(userId.Value, request);
        return Ok(result);
    }

    [HttpPut("submissions/{id:guid}")]
    [Authorize]
    public async Task<ActionResult<SubmissionResponseDto>> UpdateSubmission(
        Guid id,
        [FromBody] UpdateSubmissionRequestDto request)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await _writingService.UpdateSubmissionAsync(userId.Value, id, request);
        return Ok(result);
    }

    [HttpPost("submissions/{id:guid}/submit")]
    [Authorize]
    public async Task<ActionResult<SubmissionResponseDto>> SubmitSubmission(Guid id)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await _writingService.SubmitSubmissionAsync(userId.Value, id);
        return Ok(result);
    }

    [HttpGet("submissions")]
    [Authorize]
    public async Task<ActionResult<IReadOnlyList<SubmissionListItemDto>>> GetUserSubmissions()
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await _writingService.GetUserSubmissionsAsync(userId.Value);
        return Ok(result);
    }

    [HttpGet("submissions/{id:guid}")]
    [Authorize]
    public async Task<ActionResult<SubmissionResponseDto>> GetSubmission(Guid id)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await _writingService.GetSubmissionByIdAsync(userId.Value, id);
        return Ok(result);
    }

    private Guid? GetUserId()
    {
        var userIdValue = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        return Guid.TryParse(userIdValue, out var userId) ? userId : null;
    }
}