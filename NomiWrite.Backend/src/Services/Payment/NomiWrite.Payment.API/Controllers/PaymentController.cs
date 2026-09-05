using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NomiWrite.Payment.Application.DTOs;
using NomiWrite.Payment.Application.Interfaces;
using NomiWrite.Payment.Domain.Entities;
using NomiWrite.Payment.Domain.Enums;
using NomiWrite.Payment.Infrastructure.Services;
using NomiWrite.Shared.Contracts.Events.Payment;

namespace NomiWrite.Payment.API.Controllers;

[ApiController]
[Route("api/payment")]
public class PaymentController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly VnPayGatewayService _vnPayGateway;
    private readonly MomoGatewayService _momoGateway;
    private readonly IPaymentDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<PaymentController> _logger;

    public PaymentController(
        IPaymentService paymentService,
        VnPayGatewayService vnPayGateway,
        MomoGatewayService momoGateway,
        IPaymentDbContext dbContext,
        IPublishEndpoint publishEndpoint,
        ILogger<PaymentController> logger)
    {
        _paymentService = paymentService;
        _vnPayGateway = vnPayGateway;
        _momoGateway = momoGateway;
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<CreatePaymentResponseDto>> CreatePayment([FromBody] CreatePaymentRequestDto request)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var result = await _paymentService.CreatePaymentAsync(userId.Value, request);

        if (request.Provider == PaymentProvider.VNPay)
        {
            var order = new PaymentOrder
            {
                Amount = result.Amount,
                OrderReference = result.OrderReference,
                CreatedAt = result.CreatedAt == default ? DateTime.UtcNow : result.CreatedAt
            };

            result.PaymentUrl = await _vnPayGateway.CreatePaymentAsync(order, GetClientIpAddress());
        }
        else if (request.Provider == PaymentProvider.Momo)
        {
            var order = new PaymentOrder
            {
                Amount = result.Amount,
                OrderReference = result.OrderReference
            };

            result.PaymentUrl = await _momoGateway.CreatePaymentAsync(order);
        }

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

    [HttpGet("vnpay/ipn")]
    [AllowAnonymous]
    public async Task<IActionResult> HandleVnPayIpn()
    {
        var vnpParams = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var key in Request.Query.Keys)
        {
            if (key.StartsWith("vnp_", StringComparison.OrdinalIgnoreCase))
            {
                vnpParams[key] = Request.Query[key].ToString();
            }
        }

        var verification = _vnPayGateway.VerifyIpnAsync(vnpParams);
        if (!verification.IsSignatureValid)
        {
            _logger.LogWarning("VNPay IPN rejected: invalid signature for order {OrderReference}.", verification.OrderReference);
            return Ok(new { RspCode = "97", Message = "Invalid signature" });
        }

        if (string.IsNullOrEmpty(verification.OrderReference))
        {
            _logger.LogWarning("VNPay IPN rejected: missing transaction reference.");
            return Ok(new { RspCode = "01", Message = "Order not found" });
        }

        var payment = await _dbContext.Payments.FirstOrDefaultAsync(p => p.OrderReference == verification.OrderReference);
        if (payment is null)
        {
            _logger.LogWarning("VNPay IPN rejected: no payment with reference {OrderReference}.", verification.OrderReference);
            return Ok(new { RspCode = "01", Message = "Order not found" });
        }

        if (verification.Amount != payment.Amount)
        {
            _logger.LogWarning(
                "VNPay IPN rejected: amount {IpnAmount} does not match stored {Amount} for {OrderReference}.",
                verification.Amount,
                payment.Amount,
                payment.OrderReference);
            return Ok(new { RspCode = "04", Message = "Invalid amount" });
        }

        var success = VnPayGatewayService.IsPaymentSuccessful(verification);

        if (success && payment.Status == PaymentStatus.Completed)
        {
            await _dbContext.SaveChangesAsync();
            return Ok(new { RspCode = "00", Message = "Confirm Success" });
        }

        if (!success && payment.Status == PaymentStatus.Failed)
        {
            await _dbContext.SaveChangesAsync();
            return Ok(new { RspCode = "00", Message = "Confirm Success" });
        }

        var now = DateTime.UtcNow;
        _dbContext.PaymentTransactions.Add(new PaymentTransaction
        {
            PaymentId = payment.Id,
            ProviderTransactionId = verification.ProviderTransactionId,
            RawPayload = JsonSerializer.Serialize(vnpParams),
            ReceivedAt = now
        });

        if (success)
        {
            payment.Status = PaymentStatus.Completed;
            payment.UpdatedAt = now;
            await _dbContext.SaveChangesAsync();
            await _publishEndpoint.Publish(
                new PaymentCompletedEvent(payment.Id, payment.UserId, payment.Amount, payment.OrderReference));
        }
        else
        {
            payment.Status = PaymentStatus.Failed;
            payment.UpdatedAt = now;
            await _dbContext.SaveChangesAsync();
            await _publishEndpoint.Publish(
                new PaymentFailedEvent(payment.Id, payment.UserId, "VNPay reported a failed transaction."));
        }

        return Ok(new { RspCode = "00", Message = "Confirm Success" });
    }

    [HttpPost("momo/ipn")]
    [AllowAnonymous]
    public async Task<IActionResult> HandleMomoIpn([FromBody] MomoIpnPayload payload)
    {
        var verification = _momoGateway.VerifyIpnAsync(payload);
        if (!verification.IsSignatureValid)
        {
            _logger.LogWarning("MoMo IPN rejected: invalid signature for order {OrderId}.", verification.OrderId);
            return BadRequest();
        }

        if (string.IsNullOrEmpty(verification.OrderId))
        {
            _logger.LogWarning("MoMo IPN rejected: missing order id.");
            return BadRequest();
        }

        var payment = await _dbContext.Payments.FirstOrDefaultAsync(p => p.OrderReference == verification.OrderId);
        if (payment is null)
        {
            _logger.LogWarning("MoMo IPN rejected: no payment with reference {OrderId}.", verification.OrderId);
            return NotFound();
        }

        if (payload.Amount != payment.Amount)
        {
            _logger.LogWarning(
                "MoMo IPN rejected: amount {IpnAmount} does not match stored {Amount} for {OrderId}.",
                payload.Amount,
                payment.Amount,
                payment.OrderReference);
            return BadRequest();
        }

        var success = verification.ResultCode == 0;

        if (success && payment.Status != PaymentStatus.Completed)
        {
            var now = DateTime.UtcNow;
            _dbContext.PaymentTransactions.Add(new PaymentTransaction
            {
                PaymentId = payment.Id,
                ProviderTransactionId = verification.TransId,
                RawPayload = JsonSerializer.Serialize(payload),
                ReceivedAt = now
            });

            payment.Status = PaymentStatus.Completed;
            payment.UpdatedAt = now;
            await _dbContext.SaveChangesAsync();
            await _publishEndpoint.Publish(
                new PaymentCompletedEvent(payment.Id, payment.UserId, payment.Amount, payment.OrderReference));
        }
        else if (!success && payment.Status != PaymentStatus.Failed)
        {
            var now = DateTime.UtcNow;
            _dbContext.PaymentTransactions.Add(new PaymentTransaction
            {
                PaymentId = payment.Id,
                ProviderTransactionId = verification.TransId,
                RawPayload = JsonSerializer.Serialize(payload),
                ReceivedAt = now
            });

            payment.Status = PaymentStatus.Failed;
            payment.UpdatedAt = now;
            await _dbContext.SaveChangesAsync();
            await _publishEndpoint.Publish(
                new PaymentFailedEvent(payment.Id, payment.UserId, verification.Message));
        }

        return NoContent();
    }

    private Guid? GetUserId()
    {
        var userIdValue = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        return Guid.TryParse(userIdValue, out var userId) ? userId : null;
    }

    private string GetClientIpAddress()
    {
        var remoteIp = HttpContext.Connection.RemoteIpAddress;
        if (remoteIp is null)
        {
            return "127.0.0.1";
        }

        if (remoteIp.IsIPv4MappedToIPv6)
        {
            remoteIp = remoteIp.MapToIPv4();
        }

        return remoteIp.ToString();
    }
}
