using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NomiWrite.AICoordinator.Application.DTOs;
using NomiWrite.AICoordinator.Application.Interfaces;
using NomiWrite.AICoordinator.Domain.Enums;

namespace NomiWrite.AICoordinator.API.Controllers;

[ApiController]
[Route("api/grading")]
[Authorize]
public class GradingController : ControllerBase
{
    private readonly IGradingService _gradingService;

    public GradingController(IGradingService gradingService)
    {
        _gradingService = gradingService;
    }

    [HttpGet("submissions/{submissionId:guid}")]
    public async Task<IActionResult> GetGradingResult(Guid submissionId)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await _gradingService.GetGradingResultBySubmissionIdAsync(submissionId, userId.Value);

        if (result is null || result.Status == GradingStatus.Pending)
            return NotFound(new { success = false, message = "Grading result not found or still pending." });

        return Ok(new { success = true, data = result });
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetGradingHistory()
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var history = await _gradingService.GetGradingHistoryAsync(userId.Value);
        return Ok(new { success = true, data = history });
    }

    [HttpGet("submissions/{submissionId:guid}/compare")]
    public async Task<IActionResult> CompareWithPreviousAttempt(Guid submissionId)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var comparison = await _gradingService.CompareWithPreviousAttemptAsync(userId.Value, submissionId);
        return Ok(new { success = true, data = comparison });
    }

    [HttpPost("submissions/{submissionId:guid}/request-tutor-review")]
    public async Task<IActionResult> RequestTutorReview(Guid submissionId)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var reviewRequest = await _gradingService.RequestTutorReviewAsync(userId.Value, submissionId, GetBearerToken());
        return Ok(new { success = true, data = reviewRequest });
    }

    [HttpGet("tutor-review-requests")]
    public async Task<IActionResult> GetTutorReviewRequests()
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var requests = await _gradingService.GetTutorReviewRequestsAsync(userId.Value);
        return Ok(new { success = true, data = requests });
    }

    [HttpPost("results/{gradingResultId:guid}/flag")]
    public async Task<IActionResult> FlagGradingResult(Guid gradingResultId, [FromBody] FlagGradingResultRequestDto request)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var confirmation = await _gradingService.FlagGradingResultAsync(userId.Value, gradingResultId, request);
        return Ok(new { success = true, data = confirmation });
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
