using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

    private Guid? GetUserId()
    {
        var userIdValue = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        return Guid.TryParse(userIdValue, out var userId) ? userId : null;
    }
}
