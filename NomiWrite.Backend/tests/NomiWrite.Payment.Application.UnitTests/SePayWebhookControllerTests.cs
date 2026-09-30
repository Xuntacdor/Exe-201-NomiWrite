using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using NomiWrite.Payment.API.Controllers;
using NomiWrite.Payment.Application.DTOs;
using NomiWrite.Payment.Application.Interfaces;
using NomiWrite.Payment.Application.Services;
using NomiWrite.Payment.Infrastructure.Options;
using NomiWrite.Payment.Infrastructure.Services;

namespace NomiWrite.Payment.Application.UnitTests;

public class SePayWebhookControllerTests
{
    private const string ApiKey = "sepay-secret-key";

    private static SePayWebhookController Build(
        ISePayWebhookService webhookService,
        string apiKey = ApiKey,
        string accountNumber = "1234567890")
    {
        var sePayVietQr = new SePayVietQrService(
            Options.Create(new SePaySettings
            {
                ApiKey = apiKey,
                BankId = "970436",
                AccountNumber = accountNumber,
                AccountName = "NomiWrite Company"
            }),
            NullLogger<SePayVietQrService>.Instance);

        return new SePayWebhookController(
            sePayVietQr,
            webhookService,
            NullLogger<SePayWebhookController>.Instance);
    }

    private static DefaultHttpContext HttpContext(string? authorization, string body, string contentType = "application/json")
    {
        var context = new DefaultHttpContext();
        context.Request.Method = "POST";
        context.Request.ContentType = contentType;
        context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));
        context.Response.Body = new MemoryStream();

        if (authorization is not null)
            context.Request.Headers.Authorization = authorization;

        return context;
    }

    private static SePayWebhookController WithBody(
        ISePayWebhookService service,
        string body,
        string? authorization = null,
        string contentType = "application/json")
    {
        var controller = Build(service);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = HttpContext(authorization ?? $"Apikey {ApiKey}", body, contentType)
        };
        return controller;
    }

    /// <summary>
    /// Mirrors the exact JSON shape SePay posts so that the [JsonPropertyName] bindings
    /// are covered, rather than serialising the DTO we happen to also deserialize into.
    /// </summary>
    private static string SePayJson(
        long id = 92704,
        string? code = null,
        string content = "",
        string transferType = "in",
        long transferAmount = 100_000) => $$"""
    {
      "id": {{id}},
      "gateway": "Vietcombank",
      "transactionDate": "2024-07-02 11:08:33",
      "accountNumber": "1234567890",
      "subAccount": "",
      "code": {{Json(code)}},
      "content": {{Json(content)}},
      "transferType": "{{transferType}}",
      "description": "NGUYEN VAN A chuyen tien",
      "transferAmount": {{transferAmount}},
      "accumulated": 105000000,
      "referenceCode": "FT24012345678"
    }
    """;

    private static string Json(string? value) => value is null ? "null" : JsonSerializer.Serialize(value);

    /// <summary>
    /// Asserts the 401 the spec requires. Matched by status code rather than by result
    /// type so the assertion does not break when a diagnostic body is added to the response.
    /// </summary>
    private static void ShouldBeUnauthorized(IActionResult result)
    {
        var unauthorized = result.Should().BeAssignableTo<ObjectResult>().Subject;
        unauthorized.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
        unauthorized.Value.Should().BeEquivalentTo(new { success = false });
    }

    // ── Authorization ──────────────────────────────────────────────────────────

    [Theory]
    [InlineData("Apikey wrong-key")]
    [InlineData("Bearer sepay-secret-key")]
    [InlineData("sepay-secret-key")]
    [InlineData("apikey sepay-secret-key")]
    [InlineData("")]
    public async Task HandleWebhook_InvalidAuthorizationHeader_Returns401(string header)
    {
        var service = Substitute.For<ISePayWebhookService>();
        var controller = WithBody(service, SePayJson(), header);

        ShouldBeUnauthorized(await controller.HandleWebhook());

        // An unauthenticated caller must never reach the fulfilment path.
        await service.DidNotReceive().ProcessAsync(Arg.Any<SePayWebhookDto>(), Arg.Any<string?>());
    }

    [Fact]
    public async Task HandleWebhook_MissingAuthorizationHeader_Returns401()
    {
        var service = Substitute.For<ISePayWebhookService>();
        var controller = Build(service);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = HttpContext(null, SePayJson())
        };

        ShouldBeUnauthorized(await controller.HandleWebhook());
        await service.DidNotReceive().ProcessAsync(Arg.Any<SePayWebhookDto>(), Arg.Any<string?>());
    }

    [Fact]
    public async Task HandleWebhook_UnconfiguredApiKey_Returns503AndDoesNotFulfil()
    {
        // Fail closed: with no secret to compare against, accepting the request would
        // let anyone POST a fabricated transfer and credit themselves a plan.
        var service = Substitute.For<ISePayWebhookService>();
        var controller = Build(service, apiKey: string.Empty);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = HttpContext("Apikey anything", SePayJson())
        };

        var result = await controller.HandleWebhook();

        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status503ServiceUnavailable);
        await service.DidNotReceive().ProcessAsync(Arg.Any<SePayWebhookDto>(), Arg.Any<string?>());
    }

    // ── Payload handling ───────────────────────────────────────────────────────

    [Fact]
    public async Task HandleWebhook_AuthorizedJsonTransfer_BindsEveryFieldAndReturnsSuccess()
    {
        var orderReference = SePayOrderReference.Create();
        SePayWebhookDto? captured = null;

        var service = Substitute.For<ISePayWebhookService>();
        service.ProcessAsync(Arg.Any<SePayWebhookDto>(), Arg.Any<string?>())
            .Returns(call =>
            {
                captured = call.Arg<SePayWebhookDto>();
                return new SePayWebhookResult(SePayWebhookOutcome.Completed, orderReference, null);
            });

        var controller = WithBody(service, SePayJson(
            code: orderReference,
            content: $"{orderReference} chuyen tien",
            transferAmount: 5_000_000));

        var result = await controller.HandleWebhook();

        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(new { success = true });

        captured.Should().NotBeNull();
        captured!.Id.Should().Be(92704);
        captured.Gateway.Should().Be("Vietcombank");
        captured.TransactionDate.Should().Be("2024-07-02 11:08:33");
        captured.AccountNumber.Should().Be("1234567890");
        captured.SubAccount.Should().BeEmpty();
        captured.Code.Should().Be(orderReference);
        captured.Content.Should().Be($"{orderReference} chuyen tien");
        captured.TransferType.Should().Be("in");
        captured.Description.Should().Be("NGUYEN VAN A chuyen tien");
        captured.TransferAmount.Should().Be(5_000_000);
        captured.Accumulated.Should().Be(105_000_000);
        captured.ReferenceCode.Should().Be("FT24012345678");

        // The receiving account is handed to the service so it can reject transfers
        // that landed on an account this merchant does not collect into.
        await service.Received(1).ProcessAsync(Arg.Any<SePayWebhookDto>(), "1234567890");
    }

    [Fact]
    public async Task HandleWebhook_FormEncodedTransfer_IsAccepted()
    {
        // SePay can be configured to post form data instead of JSON, in which case
        // every value arrives as a string and the numbers need lenient parsing.
        SePayWebhookDto? captured = null;
        var service = Substitute.For<ISePayWebhookService>();
        service.ProcessAsync(Arg.Any<SePayWebhookDto>(), Arg.Any<string?>())
            .Returns(call =>
            {
                captured = call.Arg<SePayWebhookDto>();
                return new SePayWebhookResult(SePayWebhookOutcome.Completed, null, null);
            });

        var body = "id=42&transferAmount=199000&transferType=in&accountNumber=1234567890&content=NWQ0000";
        var controller = WithBody(service, body, contentType: "application/x-www-form-urlencoded");

        (await controller.HandleWebhook()).Should().BeOfType<OkObjectResult>();

        captured.Should().NotBeNull();
        captured!.Id.Should().Be(42);
        captured.TransferAmount.Should().Be(199_000);
        captured.Accumulated.Should().Be(0);
        captured.TransferType.Should().Be("in");
        captured.AccountNumber.Should().Be("1234567890");
        captured.Content.Should().Be("NWQ0000");
    }

    [Fact]
    public async Task HandleWebhook_UnmatchedTransfer_IsStillAcknowledgedWithSuccess()
    {
        // SePay retries anything that is not 200 + {"success": true}. A transfer that
        // matches no order will not match on a retry either, so it is acknowledged
        // rather than retried forever.
        var service = Substitute.For<ISePayWebhookService>();
        service.ProcessAsync(Arg.Any<SePayWebhookDto>(), Arg.Any<string?>())
            .Returns(new SePayWebhookResult(SePayWebhookOutcome.Ignored, null, "order_not_found"));

        var result = await WithBody(service, SePayJson()).HandleWebhook();

        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(new { success = true });
    }

    [Fact]
    public async Task HandleWebhook_DuplicateTransfer_IsStillAcknowledgedWithSuccess()
    {
        var service = Substitute.For<ISePayWebhookService>();
        service.ProcessAsync(Arg.Any<SePayWebhookDto>(), Arg.Any<string?>())
            .Returns(new SePayWebhookResult(SePayWebhookOutcome.Duplicate, null, "already_processed"));

        var result = await WithBody(service, SePayJson()).HandleWebhook();

        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(new { success = true });
    }

    [Theory]
    [InlineData("{ not json")]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("null")]
    public async Task HandleWebhook_UnusableJsonBody_Returns400(string body)
    {
        var service = Substitute.For<ISePayWebhookService>();

        (await WithBody(service, body).HandleWebhook()).Should().BeOfType<BadRequestObjectResult>();

        await service.DidNotReceive().ProcessAsync(Arg.Any<SePayWebhookDto>(), Arg.Any<string?>());
    }

    [Theory]
    [InlineData("application/xml")]
    [InlineData("text/plain")]
    public async Task HandleWebhook_UnsupportedContentType_Returns415(string contentType)
    {
        var service = Substitute.For<ISePayWebhookService>();
        var controller = WithBody(service, "<xml/>", contentType: contentType);

        var result = await controller.HandleWebhook();

        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status415UnsupportedMediaType);
        await service.DidNotReceive().ProcessAsync(Arg.Any<SePayWebhookDto>(), Arg.Any<string?>());
    }

    [Fact]
    public async Task HandleWebhook_UnauthorizedHeaderWinsOverAMalformedBody()
    {
        // Auth is checked first so a probe cannot distinguish body parsing from
        // credential validity, and never reaches the fulfilment path either way.
        var service = Substitute.For<ISePayWebhookService>();
        var controller = WithBody(service, "{ not json", authorization: "Apikey wrong-key");

        ShouldBeUnauthorized(await controller.HandleWebhook());
    }
}
