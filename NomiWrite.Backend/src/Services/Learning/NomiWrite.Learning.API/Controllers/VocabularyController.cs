using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NomiWrite.Learning.Application.DTOs;
using NomiWrite.Learning.Application.Interfaces;

namespace NomiWrite.Learning.API.Controllers;

[ApiController]
[Route("api/vocabulary")]
[Authorize]
public class VocabularyController : ControllerBase
{
    private readonly IVocabularyService _vocabularyService;

    public VocabularyController(IVocabularyService vocabularyService)
    {
        _vocabularyService = vocabularyService;
    }

    [HttpGet]
    public async Task<IActionResult> GetVocabulary(
        [FromQuery] Guid? submissionId,
        [FromQuery] string? topic,
        [FromQuery] bool? isMastered,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await _vocabularyService.GetAllAsync(userId.Value, submissionId, topic, isMastered, page, pageSize);
        return Ok(new { success = true, data = result });
    }

    [HttpPatch("{id:guid}/mastered")]
    public async Task<IActionResult> UpdateMastered(Guid id, [FromBody] UpdateMasteredRequestDto request)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await _vocabularyService.UpdateMasteredAsync(userId.Value, id, request.IsMastered);
        return Ok(new { success = true, data = result });
    }

    [HttpPost("groups")]
    public async Task<IActionResult> CreateGroup([FromBody] CreateVocabGroupRequestDto request)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await _vocabularyService.CreateGroupAsync(userId.Value, request);
        return Ok(new { success = true, data = result });
    }

    [HttpGet("groups")]
    public async Task<IActionResult> ListGroups()
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await _vocabularyService.ListGroupsAsync(userId.Value);
        return Ok(new { success = true, data = result });
    }

    [HttpPost("groups/{groupId:guid}/items")]
    public async Task<IActionResult> AddToGroup(Guid groupId, [FromBody] AddToVocabGroupRequestDto request)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        await _vocabularyService.AddToGroupAsync(userId.Value, groupId, request);
        return Ok(new { success = true });
    }

    [HttpDelete("groups/{groupId:guid}/items/{vocabId:guid}")]
    public async Task<IActionResult> RemoveFromGroup(Guid groupId, Guid vocabId)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        await _vocabularyService.RemoveFromGroupAsync(userId.Value, groupId, vocabId);
        return Ok(new { success = true });
    }

    private Guid? GetUserId()
    {
        var userIdValue = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        return Guid.TryParse(userIdValue, out var userId) ? userId : null;
    }
}