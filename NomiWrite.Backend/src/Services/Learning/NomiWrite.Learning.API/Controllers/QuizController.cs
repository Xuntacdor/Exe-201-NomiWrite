using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NomiWrite.Learning.Application.DTOs;
using NomiWrite.Learning.Application.Interfaces;

namespace NomiWrite.Learning.API.Controllers;

[ApiController]
[Route("api/quizzes")]
[Authorize]
public class QuizController : ControllerBase
{
    private readonly IQuizService _quizService;

    public QuizController(IQuizService quizService)
    {
        _quizService = quizService;
    }

    [HttpPost("generate")]
    public async Task<IActionResult> GenerateQuiz([FromBody] GenerateQuizRequestDto request)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await _quizService.GenerateQuizAsync(userId.Value, request);
        return Ok(new { success = true, data = result });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetQuiz(Guid id)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await _quizService.GetQuizAsync(userId.Value, id);
        return Ok(new { success = true, data = result });
    }

    [HttpPost("attempts")]
    public async Task<IActionResult> SubmitAttempt([FromBody] SubmitQuizAttemptRequestDto request)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await _quizService.SubmitAttemptAsync(userId.Value, request);
        return Ok(new { success = true, data = result });
    }

    private Guid? GetUserId()
    {
        var userIdValue = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        return Guid.TryParse(userIdValue, out var userId) ? userId : null;
    }
}