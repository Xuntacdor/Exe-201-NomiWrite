using System.Security.Claims;
using FluentAssertions;
using FluentValidation;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using NomiWrite.Payment.API.Controllers;
using NomiWrite.Payment.Application.DTOs;
using NomiWrite.Payment.Application.Exceptions;
using NomiWrite.Payment.Application.Interfaces;
using NomiWrite.Payment.Application.Services;
using NomiWrite.Payment.Application.Validation;
using NomiWrite.Payment.Application.UnitTests.Persistence;
using NomiWrite.Payment.Domain.Entities;
using NomiWrite.Payment.Domain.Enums;
using NomiWrite.Payment.Infrastructure.Options;
using NomiWrite.Payment.Infrastructure.Services;

namespace NomiWrite.Payment.Application.UnitTests;

/// <summary>
/// Covers the VietQR checkout surface: the short bank-transfer reference, the
/// bank details returned to the storefront, and the status endpoint it polls.
/// </summary>
public class VietQrCheckoutTests
{
    private const string ApiKey = "sepay-api-key-test";
    private const string BankId = "970436";
    private const string BankName = "VPBank";
    private const string AccountNumber = "1234567890";
    private const string AccountName = "NOMIWRITE";

    private static readonly Guid PlanId = Guid.NewGuid();

    private static SePayVietQrService SePay() => new(
        Options.Create(new SePaySettings
        {
            ApiKey = ApiKey,
            BankId = BankId,
            BankName = BankName,
            AccountNumber = AccountNumber,
            AccountName = AccountName
        }),
        NullLogger<SePayVietQrService>.Instance);

    private static PaymentService Service(TestPaymentDbContext db, IPublishEndpoint? publish = null)
    {
        publish ??= Substitute.For<IPublishEndpoint>();
        return new PaymentService(
            db,
            Substitute.For<IPaymentGatewayService>(),
            new Valid<CreatePaymentRequestDto>(),
            new Valid<CreateRefundRequestDto>(),
            Substitute.For<IPromoCodeValidator>(),
            publish,
            PlanClient());
    }

    private static ISubscriptionPlanClient PlanClient()
    {
        var client = Substitute.For<ISubscriptionPlanClient>();
        client.GetActivePlanAsync(PlanId).Returns(new SubscriptionPlanPrice(199_000m, "VND"));
        return client;
    }

    /// <summary>Validator that accepts everything; CreatePayment is not what's under test here.</summary>
    private sealed class Valid<T> : FluentValidation.AbstractValidator<T>
    {
    }

    private static CreatePaymentRequestDto Request(PaymentProvider provider) => new()
    {
        PlanId = PlanId,
        Provider = provider,
        Currency = "VND"
    };

    private static PaymentOrder SeedOrder(
        TestPaymentDbContext db,
        Guid userId,
        PaymentStatus status = PaymentStatus.Pending,
        string? orderReference = null)
    {
        var now = DateTime.UtcNow;
        var order = new PaymentOrder
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Amount = 199_000m,
            Currency = "VND",
            Provider = PaymentProvider.VietQR,
            Status = status,
            OrderReference = orderReference ?? SePayOrderReference.Create(),
            PlanId = PlanId,
            CreatedAt = now,
            UpdatedAt = now
        };
        db.Payments.Add(order);
        db.SaveChanges();
        return order;
    }

    // ── Order reference per provider ───────────────────────────────────────────

    [Fact]
    public async Task CreatePayment_VietQr_UsesTheShortBankTransferReference()
    {
        var db = TestPaymentDbContext.Create();

        var result = await Service(db).CreatePaymentAsync(Guid.NewGuid(), Request(PaymentProvider.VietQR));

        // A PAY-{guid} reference contains a '-' and is 36 characters, which VietQR
        // will not carry intact through a transfer. The customer must be able to
        // reproduce this string on their banking app.
        result.OrderReference.Should().MatchRegex("^NWQ[0-9A-Z]{20}$");
        result.PaymentUrl.Should().BeNull();
        result.Provider.Should().Be(PaymentProvider.VietQR);
        result.Status.Should().Be(PaymentStatus.Pending);

        db.Payments.Should().ContainSingle()
            .Which.OrderReference.Should().Be(result.OrderReference);
    }

    [Theory]
    [InlineData(PaymentProvider.VNPay)]
    [InlineData(PaymentProvider.Momo)]
    public async Task CreatePayment_OtherProviders_KeepTheLegacyPayReference(PaymentProvider provider)
    {
        var db = TestPaymentDbContext.Create();

        var result = await Service(db).CreatePaymentAsync(Guid.NewGuid(), Request(provider));

        result.OrderReference.Should().MatchRegex("^PAY-[0-9A-F]{32}$");
    }

    // ── VietQR checkout payload ────────────────────────────────────────────────

    [Fact]
    public async Task CreatePayment_VietQr_ResponseCarriesBankDetailsAndNoHostedUrl()
    {
        var db = TestPaymentDbContext.Create();

        // Driven through the real service so the reference rendered in the QR is
        // exactly the reference persisted for the webhook to match against.
        var controller = BuildController(db, Service(db));

        var action = await controller.CreatePayment(Request(PaymentProvider.VietQR));
        var ok = action.Result.Should().BeOfType<OkObjectResult>().Subject;
        var dto = ok.Value.Should().BeOfType<CreatePaymentResponseDto>().Subject;

        dto.PaymentUrl.Should().BeNull();
        dto.VietQr.Should().NotBeNull();
        dto.VietQr!.BankId.Should().Be(BankId);
        dto.VietQr.BankName.Should().Be(BankName);
        dto.VietQr.AccountNumber.Should().Be(AccountNumber);
        dto.VietQr.AccountName.Should().Be(AccountName);
        dto.VietQr.Amount.Should().Be(199_000m);
        dto.VietQr.TransferContent.Should().Be(dto.OrderReference);
        dto.VietQr.QrImageUrl.Should().Contain(dto.OrderReference);

        // The amount in the QR must match what the webhook later compares against.
        dto.VietQr.Amount.Should().Be(db.Payments.Single().Amount);
    }

    private static PaymentController BuildController(TestPaymentDbContext db, IPaymentService paymentService)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(
            new[] { new Claim("sub", Guid.NewGuid().ToString()) },
            authenticationType: "test"));

        var controller = new PaymentController(
            paymentService,
            // The VietQR branch never touches the hosted gateways.
            vnPayGateway: null!,
            momoGateway: null!,
            sePayVietQr: SePay(),
            db,
            Substitute.For<IPublishEndpoint>(),
            NullLogger<PaymentController>.Instance)
        {
            ControllerContext = new ControllerContext { HttpContext = httpContext }
        };

        return controller;
    }

    // ── Request validation ─────────────────────────────────────────────────────

    [Theory]
    [InlineData(PaymentProvider.VNPay, true)]
    [InlineData(PaymentProvider.Momo, true)]
    [InlineData(PaymentProvider.VietQR, true)]
    public void CreatePaymentValidator_AcceptsEverySupportedProvider(PaymentProvider provider, bool expected)
    {
        // VietQR must be accepted here or the new checkout is unreachable through the API.
        var result = new CreatePaymentRequestValidator().Validate(new CreatePaymentRequestDto
        {
            PlanId = PlanId,
            Provider = provider,
            Amount = 199_000m,
            Currency = "VND"
        });

        result.IsValid.Should().Be(expected);
    }

    // ── Status polling ─────────────────────────────────────────────────────────

    [Fact]
    public async Task GetOrderStatus_ByOrderReference_ResolvesTheOrder()
    {
        var db = TestPaymentDbContext.Create();
        var userId = Guid.NewGuid();
        var order = SeedOrder(db, userId, status: PaymentStatus.Completed);

        var result = await Service(db).GetOrderStatusAsync(userId, order.OrderReference);

        result.Status.Should().Be(PaymentStatus.Completed);
        result.OrderReference.Should().Be(order.OrderReference);
    }

    [Fact]
    public async Task GetOrderStatus_ByReference_IsCaseInsensitiveAndTrimmed()
    {
        var db = TestPaymentDbContext.Create();
        var userId = Guid.NewGuid();
        var order = SeedOrder(db, userId);

        var result = await Service(db)
            .GetOrderStatusAsync(userId, $"  {order.OrderReference.ToLowerInvariant()}  ");

        result.Status.Should().Be(PaymentStatus.Pending);
    }

    [Fact]
    public async Task GetOrderStatus_ByPaymentId_ResolvesTheOrder()
    {
        var db = TestPaymentDbContext.Create();
        var userId = Guid.NewGuid();
        var order = SeedOrder(db, userId);

        var result = await Service(db).GetOrderStatusAsync(userId, order.Id.ToString());

        result.OrderReference.Should().Be(order.OrderReference);
    }

    [Fact]
    public async Task GetOrderStatus_StillPending_BeforeSePayConfirmsTheTransfer()
    {
        var db = TestPaymentDbContext.Create();
        var userId = Guid.NewGuid();
        var order = SeedOrder(db, userId, status: PaymentStatus.Pending);

        var result = await Service(db).GetOrderStatusAsync(userId, order.OrderReference);

        result.Status.Should().Be(PaymentStatus.Pending);
    }

    [Fact]
    public async Task GetOrderStatus_UnknownReference_ThrowsNotFound()
    {
        var db = TestPaymentDbContext.Create();
        SeedOrder(db, Guid.NewGuid());

        await FluentActions.Invoking(() => Service(db)
                .GetOrderStatusAsync(Guid.NewGuid(), SePayOrderReference.Create()))
            .Should().ThrowAsync<PaymentNotFoundException>();
    }

    [Fact]
    public async Task GetOrderStatus_AnotherUsersOrder_IsRejectedWith403()
    {
        var db = TestPaymentDbContext.Create();
        var order = SeedOrder(db, Guid.NewGuid());

        // Without this the polling endpoint would expose any user's payment state,
        // and more importantly let anyone probe reference numbers.
        await FluentActions.Invoking(() => Service(db)
                .GetOrderStatusAsync(Guid.NewGuid(), order.OrderReference))
            .Should().ThrowAsync<InvalidRefundException>()
            .WithMessage("*does not belong*");
    }

    [Fact]
    public async Task GetOrderStatus_AfterWebhook_MovesToCompleted()
    {
        // The dialog stops polling when it sees Completed; this is that transition.
        var db = TestPaymentDbContext.Create();
        var publish = Substitute.For<IPublishEndpoint>();
        var userId = Guid.NewGuid();
        var order = SeedOrder(db, userId, status: PaymentStatus.Pending);

        await new SePayWebhookService(db, publish, NullLogger<SePayWebhookService>.Instance)
            .ProcessAsync(new SePayWebhookDto
            {
                Id = 1,
                AccountNumber = AccountNumber,
                Code = order.OrderReference,
                Content = order.OrderReference,
                TransferType = "in",
                TransferAmount = 199_000
            }, AccountNumber);

        var result = await Service(db, publish).GetOrderStatusAsync(userId, order.OrderReference);

        result.Status.Should().Be(PaymentStatus.Completed);
    }
}
