using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NomiWrite.Payment.Application.DTOs;
using NomiWrite.Payment.Application.Interfaces;
using NomiWrite.Payment.Domain.Enums;

namespace NomiWrite.Payment.API.Controllers;

[ApiController]
[Route("api/payment")]
public class PaymentController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentController(IPaymentService paymentService) => _paymentService = paymentService;

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<CreatePaymentResponseDto>> CreatePayment([FromBody] CreatePaymentRequestDto request)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await _paymentService.CreatePaymentAsync(userId.Value, request);
        return Ok(result);
    }

    [HttpGet("{paymentId:guid}")]
    [Authorize]
    public async Task<ActionResult<PaymentStatusResponseDto>> GetPaymentStatus(Guid paymentId)
    {
        var result = await _paymentService.GetPaymentStatusAsync(paymentId);
        return Ok(result);
    }

    [HttpPost("webhook/{provider}")]
    [AllowAnonymous]
    public async Task<IActionResult> HandleWebhook(string provider)
    {
        if (!Enum.TryParse(provider, ignoreCase: true, out PaymentProvider paymentProvider))
        {
            return BadRequest(new
            {
                success = false,
                message = $"Unknown payment provider '{provider}'.",
                errors = Array.Empty<string>()
            });
        }

        var rawPayload = await new StreamReader(Request.Body).ReadToEndAsync();

        WebhookCallbackDto callback;
        try
        {
            callback = JsonSerializer.Deserialize<WebhookCallbackDto>(
                rawPayload,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? new WebhookCallbackDto();
        }
        catch (JsonException)
        {
            return BadRequest(new
            {
                success = false,
                message = "Invalid webhook payload.",
                errors = Array.Empty<string>()
            });
        }

        callback.Provider = paymentProvider;
        callback.RawPayload = rawPayload;
        callback.ReceivedAt = DateTime.UtcNow;

        var result = await _paymentService.HandleWebhookAsync(callback);
        return Ok(result);
    }

    private Guid? GetUserId()
    {
        var userIdValue = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        return Guid.TryParse(userIdValue, out var userId) ? userId : null;
    }
}