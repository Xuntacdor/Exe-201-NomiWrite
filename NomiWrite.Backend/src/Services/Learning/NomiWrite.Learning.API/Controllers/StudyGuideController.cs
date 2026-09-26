using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NomiWrite.Learning.Application.DTOs;
using NomiWrite.Learning.Application.Interfaces;

namespace NomiWrite.Learning.API.Controllers;

[ApiController]
[Route("api/study-guides")]
[Authorize]
public class StudyGuideController : ControllerBase
{
    private readonly IStudyGuideService _studyGuideService;

    public StudyGuideController(IStudyGuideService studyGuideService)
    {
        _studyGuideService = studyGuideService;
    }

    /// <summary>
    /// Returns the latest cached study guide without re-running the LLM.
    /// 404 when none has been generated yet — call the generate endpoint first.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetLatestGuide()
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var guide = await _studyGuideService.GetLatestGuideAsync(userId.Value);

        if (guide is null)
            return NotFound(new { success = false, message = "No study guide yet. Generate one first." });

        return Ok(new { success = true, data = guide });
    }

    /// <summary>
    /// Analyzes the user's writing history and generates (or refreshes) a
    /// personalized study guide. The result is cached in the database, so this
    /// is only intended to be called once per plan refresh.
    /// </summary>
    [HttpPost("generate")]
    public async Task<IActionResult> GenerateGuide([FromBody] GenerateStudyGuideRequestDto request)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var guide = await _studyGuideService.GenerateGuideAsync(userId.Value, request, GetBearerToken());
        return Ok(new { success = true, data = guide });
    }

    private Guid? GetUserId()
    {
        var userIdValue = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        return Guid.TryParse(userIdValue, out var userId) ? userId : null;
    }

    private string? GetBearerToken()
    {
        var authorizationHeader = Request.Headers.Authorization.ToString();

        if (string.IsNullOrWhiteSpace(authorizationHeader))
            return null;

        const string bearerPrefix = "Bearer ";
        return authorizationHeader.StartsWith(bearerPrefix, StringComparison.OrdinalIgnoreCase)
            ? authorizationHeader[bearerPrefix.Length..]
            : authorizationHeader;
    }
}