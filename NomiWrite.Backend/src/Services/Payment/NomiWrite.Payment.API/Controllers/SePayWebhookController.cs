using System.Globalization;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using NomiWrite.Payment.Application.DTOs;
using NomiWrite.Payment.Application.Interfaces;
using NomiWrite.Payment.Infrastructure.Services;

namespace NomiWrite.Payment.API.Controllers;

/// <summary>
/// Receives SePay bank-transfer webhooks and confirms VietQR orders.
/// Spec: https://developer.sepay.vn/vi/sepay-webhooks/tich-hop-webhook
/// </summary>
[ApiController]
[Route("api/payment")]
public class SePayWebhookController : ControllerBase
{
    private const string JsonContentType = "application/json";
    private const string FormUrlEncodedContentType = "application/x-www-form-urlencoded";
    private const string MultipartContentType = "multipart/form-data";

    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly SePayVietQrService _sePayVietQr;
    private readonly ISePayWebhookService _webhookService;
    private readonly ILogger<SePayWebhookController> _logger;

    public SePayWebhookController(
        SePayVietQrService sePayVietQr,
        ISePayWebhookService webhookService,
        ILogger<SePayWebhookController> logger)
    {
        _sePayVietQr = sePayVietQr;
        _webhookService = webhookService;
        _logger = logger;
    }

    [HttpPost("sepay-webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> HandleWebhook()
    {
        if (!_sePayVietQr.IsAuthorized(Request.Headers.Authorization.ToString()))
        {
            // Fail closed: an unconfigured API key must never be treated as a match.
            return _sePayVietQr.IsConfigured
                ? Unauthorized(new { success = false, message = "Unauthorized" })
                : StatusCode(StatusCodes.Status503ServiceUnavailable, new
                {
                    success = false,
                    message = "SePay webhook is not configured on this server."
                });
        }

        var (payload, error) = await TryReadPayloadAsync();
        if (error is not null)
            return error;

        // SePay only counts 200/201 with exactly {"success": true} as delivered, and
        // retries anything else. A transfer that matches no order will never match on
        // a retry either, so unmatched traffic is acknowledged and left in the logs
        // rather than generating a retry storm. Genuine failures still surface as 5xx.
        var result = await _webhookService.ProcessAsync(payload, _sePayVietQr.ExpectedAccountNumber);

        if (result.Outcome == SePayWebhookOutcome.Ignored)
        {
            _logger.LogWarning(
                "SePay webhook {SePayId} acknowledged without fulfilment: {Reason}.",
                payload.Id,
                result.Reason);
        }

        return Ok(new { success = true });
    }

    private async Task<(SePayWebhookDto Payload, IActionResult? Error)> TryReadPayloadAsync()
    {
        if (Request.HasJsonContentType())
        {
            // The raw body is read rather than model-bound so that adding SePay's
            // HMAC-SHA256 signature check later stays a local change: that scheme
            // signs the untouched bytes, not a re-serialised object.
            using var reader = new StreamReader(Request.Body, Encoding.UTF8);
            var rawBody = await reader.ReadToEndAsync();

            if (string.IsNullOrWhiteSpace(rawBody))
                return (null!, Malformed("Empty request body."));

            try
            {
                var payload = JsonSerializer.Deserialize<SePayWebhookDto>(rawBody, SerializerOptions);
                if (payload is null)
                    return (null!, Malformed("Malformed JSON payload."));

                return (payload, null);
            }
            catch (JsonException exception)
            {
                _logger.LogWarning(exception, "SePay webhook body is not valid JSON.");
                return (null!, Malformed("Malformed JSON payload."));
            }
        }

        var contentType = Request.ContentType ?? string.Empty;
        if (contentType.StartsWith(FormUrlEncodedContentType, StringComparison.OrdinalIgnoreCase)
            || contentType.StartsWith(MultipartContentType, StringComparison.OrdinalIgnoreCase))
        {
            return (FromForm(await Request.ReadFormAsync()), null);
        }

        _logger.LogWarning("SePay webhook sent unsupported Content-Type '{ContentType}'.", contentType);
        return (
            null!,
            StatusCode(StatusCodes.Status415UnsupportedMediaType, new
            {
                success = false,
                message = "Expected application/json, application/x-www-form-urlencoded or multipart/form-data."
            }));
    }

    private IActionResult Malformed(string message) =>
        BadRequest(new { success = false, message });

    /// <summary>
    /// SePay can be configured to post form data instead of JSON. Every value then
    /// arrives as a string, so the numeric fields are parsed leniently here.
    /// </summary>
    private static SePayWebhookDto FromForm(IFormCollection form) => new()
    {
        Id = ParseLong(form["id"]),
        Gateway = form["gateway"],
        TransactionDate = form["transactionDate"],
        AccountNumber = form["accountNumber"],
        SubAccount = form["subAccount"],
        Code = form["code"],
        Content = form["content"],
        TransferType = form["transferType"],
        Description = form["description"],
        TransferAmount = ParseLong(form["transferAmount"]),
        Accumulated = ParseLong(form["accumulated"]),
        ReferenceCode = form["referenceCode"]
    };

    private static long ParseLong(StringValues values)
    {
        var raw = values.ToString().Trim();
        return long.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : 0L;
    }
}
