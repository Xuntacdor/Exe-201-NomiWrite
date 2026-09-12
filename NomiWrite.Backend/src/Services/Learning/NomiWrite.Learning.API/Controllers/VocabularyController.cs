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
        [FromQuery] string? topic,
        [FromQuery] bool? isMastered,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await _vocabularyService.GetAllAsync(userId.Value, topic, isMastered, page, pageSize);
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

    private Guid? GetUserId()
    {
        var userIdValue = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        return Guid.TryParse(userIdValue, out var userId) ? userId : null;
    }
}