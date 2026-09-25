using FluentAssertions;
using FluentValidation;
using MassTransit;
using NSubstitute;
using NomiWrite.Payment.Application.DTOs;
using NomiWrite.Payment.Application.Exceptions;
using NomiWrite.Payment.Application.Interfaces;
using NomiWrite.Payment.Application.Services;
using NomiWrite.Payment.Application.UnitTests.Persistence;
using NomiWrite.Payment.Domain.Entities;
using NomiWrite.Payment.Domain.Enums;
using NomiWrite.Shared.Contracts.Events.Payment;

namespace NomiWrite.Payment.Application.UnitTests;

public class PaymentServiceTests
{
    private static readonly Guid PlanId = Guid.NewGuid();
    private static IPaymentGatewayService ValidGateway()
    {
        var gateway = Substitute.For<IPaymentGatewayService>();
        gateway.CreatePaymentAsync(Arg.Any<PaymentOrder>())
            .Returns(new GatewayCreatePaymentResult("tx-1", "https://pay.example"));
        gateway.VerifyWebhookAsync(Arg.Any<WebhookCallbackDto>())
            .Returns(new GatewayWebhookVerificationResult(true, null));
        return gateway;
    }

    private static PaymentService Build(TestPaymentDbContext db, IPaymentGatewayService? gateway = null,
        IPromoCodeValidator? promo = null, IPublishEndpoint? publish = null,
        ISubscriptionPlanClient? planClient = null)
    {
        gateway ??= ValidGateway();
        promo ??= Substitute.For<IPromoCodeValidator>();
        publish ??= Substitute.For<IPublishEndpoint>();
        planClient ??= Substitute.For<ISubscriptionPlanClient>();
        planClient.GetActivePlanAsync(PlanId).Returns(new SubscriptionPlanPrice(100_000m, "VND"));

        return new PaymentService(
            db,
            gateway,
            new Valid<CreatePaymentRequestDto>(),
            new Valid<CreateRefundRequestDto>(),
            promo,
            publish,
            planClient);
    }

    private static PaymentOrder SeedPayment(TestPaymentDbContext db, Guid userId,
        PaymentStatus status = PaymentStatus.Completed, PaymentProvider provider = PaymentProvider.VNPay,
        decimal amount = 100_000m, string currency = "VND")
    {
        var now = DateTime.UtcNow;
        var payment = new PaymentOrder
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Amount = amount,
            Currency = currency,
            Provider = provider,
            Status = status,
            OrderReference = $"PAY-{Guid.NewGuid():N}".ToUpperInvariant(),
            CreatedAt = now,
            UpdatedAt = now
        };
        db.Payments.Add(payment);
        db.SaveChanges();
        return payment;
    }

    private static WebhookCallbackDto CallbackFor(PaymentOrder payment, bool success = true,
        PaymentProvider? provider = null, string reason = "test") => new()
    {
        Provider = provider ?? payment.Provider,
        OrderReference = payment.OrderReference,
        ProviderTransactionId = "provider-tx-1",
        IsSuccess = success,
        Reason = reason,
        RawPayload = "{\"ok\":true}",
        ReceivedAt = DateTime.UtcNow
    };

    #region U-P1 — webhook idempotency

    [Fact]
    public async Task HandleWebhookAsync_DuplicateSuccessCallback_DoesNotChangeStatusOrPublish()
    {
        var db = TestPaymentDbContext.Create();
        var payment = SeedPayment(db, Guid.NewGuid(), status: PaymentStatus.Completed);
        var publish = Substitute.For<IPublishEndpoint>();

        var sut = Build(db, publish: publish);
        var result = await sut.HandleWebhookAsync(CallbackFor(payment));

        result.Status.Should().Be(PaymentStatus.Completed);
        db.Payments.Single().Status.Should().Be(PaymentStatus.Completed);

        var completedEvents = publish.ReceivedCalls()
            .SelectMany(c => c.GetArguments()).OfType<PaymentCompletedEvent>();
        var failedEvents = publish.ReceivedCalls()
            .SelectMany(c => c.GetArguments()).OfType<PaymentFailedEvent>();
        completedEvents.Should().BeEmpty();
        failedEvents.Should().BeEmpty();

        db.PaymentTransactions.Should().BeEmpty();
    }

    #endregion

    #region U-P2 — provider-mismatch webhook

    [Fact]
    public async Task HandleWebhookAsync_ProviderMismatch_Throws()
    {
        var db = TestPaymentDbContext.Create();
        var payment = SeedPayment(db, Guid.NewGuid(), provider: PaymentProvider.VNPay);

        var sut = Build(db);
        var act = () => sut.HandleWebhookAsync(CallbackFor(payment, provider: PaymentProvider.Momo));

        await act.Should().ThrowAsync<InvalidWebhookException>();
        db.Payments.Single().Status.Should().Be(PaymentStatus.Completed);
    }

    [Fact]
    public async Task HandleWebhookAsync_UnknownOrderReference_ThrowsPaymentNotFound()
    {
        var db = TestPaymentDbContext.Create();

        var sut = Build(db);
        var act = () => sut.HandleWebhookAsync(new WebhookCallbackDto
        {
            Provider = PaymentProvider.VNPay,
            OrderReference = "PAY-UNKNOWN"
        });

        await act.Should().ThrowAsync<PaymentNotFoundException>();
    }

    #endregion

    #region Webhook happy/failure paths

    [Fact]
    public async Task HandleWebhookAsync_Success_CompletesPaymentAndPublishes()
    {
        var db = TestPaymentDbContext.Create();
        var payment = SeedPayment(db, Guid.NewGuid(), status: PaymentStatus.Pending);
        var publish = Substitute.For<IPublishEndpoint>();

        var sut = Build(db, publish: publish);
        var result = await sut.HandleWebhookAsync(CallbackFor(payment));

        result.Status.Should().Be(PaymentStatus.Completed);
        var completed = publish.ReceivedCalls()
            .SelectMany(c => c.GetArguments()).OfType<PaymentCompletedEvent>().Single();
        completed.PaymentId.Should().Be(payment.Id);
        completed.Amount.Should().Be(payment.Amount);
        db.PaymentTransactions.Should().ContainSingle(t => t.PaymentId == payment.Id);
    }

    [Fact]
    public async Task HandleWebhookAsync_Failure_MarksFailedAndPublishesFailed()
    {
        var db = TestPaymentDbContext.Create();
        var payment = SeedPayment(db, Guid.NewGuid(), status: PaymentStatus.Pending);
        var publish = Substitute.For<IPublishEndpoint>();

        var sut = Build(db, publish: publish);
        var result = await sut.HandleWebhookAsync(CallbackFor(payment, success: false, reason: "Insufficient funds"));

        result.Status.Should().Be(PaymentStatus.Failed);
        var failed = publish.ReceivedCalls()
            .SelectMany(c => c.GetArguments()).OfType<PaymentFailedEvent>().Single();
        failed.Reason.Should().Be("Insufficient funds");
    }

    [Fact]
    public async Task HandleWebhookAsync_InvalidGatewaySignature_Throws()
    {
        var db = TestPaymentDbContext.Create();
        var payment = SeedPayment(db, Guid.NewGuid(), status: PaymentStatus.Pending);

        var gateway = ValidGateway();
        gateway.VerifyWebhookAsync(Arg.Any<WebhookCallbackDto>())
            .Returns(new GatewayWebhookVerificationResult(false, "HMAC mismatch"));

        var sut = Build(db, gateway: gateway);
        var act = () => sut.HandleWebhookAsync(CallbackFor(payment));

        await act.Should().ThrowAsync<InvalidWebhookException>();
        db.Payments.Single().Status.Should().Be(PaymentStatus.Pending);
    }

    #endregion

    #region U-P5 — create payment discount math

    [Fact]
    public async Task CreatePaymentAsync_ValidPromo_AppliesDiscount()
    {
        var db = TestPaymentDbContext.Create();
        var promo = Substitute.For<IPromoCodeValidator>();
        promo.ValidateAsync("SAVE20").Returns(new PromoCodeValidationResult(true, 20));

        var sut = Build(db, promo: promo);
        var result = await sut.CreatePaymentAsync(Guid.NewGuid(), new CreatePaymentRequestDto
        {
            Amount = 100_000m,
            Provider = PaymentProvider.VNPay,
            PlanId = PlanId,
            PromoCode = "SAVE20"
        });

        result.Amount.Should().Be(80_000m);
        result.AppliedDiscountPercent.Should().Be(20);
        db.Payments.Single().AppliedPromoCode.Should().Be("SAVE20");
        result.Currency.Should().Be("VND");
        result.Status.Should().Be(PaymentStatus.Pending);
        result.OrderReference.Should().StartWith("PAY-");

        db.Payments.Should().ContainSingle(p => p.Amount == 80_000m && p.AppliedDiscountPercent == 20);
    }

    [Fact]
    public async Task CreatePaymentAsync_UsesCatalogPriceInsteadOfClientAmount()
    {
        var db = TestPaymentDbContext.Create();
        var sut = Build(db);

        var result = await sut.CreatePaymentAsync(Guid.NewGuid(), new CreatePaymentRequestDto
        {
            Amount = 1m,
            PlanId = PlanId,
            Provider = PaymentProvider.VNPay
        });

        result.Amount.Should().Be(100_000m);
        db.Payments.Single().Amount.Should().Be(100_000m);
    }

    [Fact]
    public async Task CreatePaymentAsync_UnknownPlan_DoesNotCreateOrder()
    {
        var db = TestPaymentDbContext.Create();
        var sut = Build(db);

        var act = () => sut.CreatePaymentAsync(Guid.NewGuid(), new CreatePaymentRequestDto
        {
            Amount = 1m,
            PlanId = Guid.NewGuid(),
            Provider = PaymentProvider.Momo
        });

        await act.Should().ThrowAsync<ValidationException>();
        db.Payments.Should().BeEmpty();
    }

    [Fact]
    public async Task CreatePaymentAsync_InvalidPromo_FailsOpenNoDiscount()
    {
        var db = TestPaymentDbContext.Create();
        var promo = Substitute.For<IPromoCodeValidator>();
        promo.ValidateAsync("BAD").Returns(new PromoCodeValidationResult(false, null));

        var sut = Build(db, promo: promo);
        var result = await sut.CreatePaymentAsync(Guid.NewGuid(), new CreatePaymentRequestDto
        {
            Amount = 100_000m,
            Provider = PaymentProvider.VNPay,
            PlanId = PlanId,
            PromoCode = "BAD"
        });

        result.Amount.Should().Be(100_000m);
        result.AppliedDiscountPercent.Should().BeNull();
    }

    [Fact]
    public async Task CreatePaymentAsync_OutOfRangeDiscount_FailsOpenNoDiscount()
    {
        var db = TestPaymentDbContext.Create();
        var promo = Substitute.For<IPromoCodeValidator>();
        promo.ValidateAsync("FRAUD").Returns(new PromoCodeValidationResult(true, 150));

        var sut = Build(db, promo: promo);
        var result = await sut.CreatePaymentAsync(Guid.NewGuid(), new CreatePaymentRequestDto
        {
            Amount = 100_000m,
            Provider = PaymentProvider.VNPay,
            PlanId = PlanId,
            PromoCode = "FRAUD"
        });

        result.Amount.Should().Be(100_000m);
        result.AppliedDiscountPercent.Should().BeNull();
    }

    [Fact]
    public async Task CreatePaymentAsync_NormalizesCurrencyAndGeneratesUniqueReferences()
    {
        var db = TestPaymentDbContext.Create();

        var sut = Build(db);
        var first = await sut.CreatePaymentAsync(Guid.NewGuid(), new CreatePaymentRequestDto
        {
            Amount = 100m,
            Currency = "vnd",
            PlanId = PlanId,
            Provider = PaymentProvider.Momo
        });
        var second = await sut.CreatePaymentAsync(Guid.NewGuid(), new CreatePaymentRequestDto
        {
            Amount = 100m,
            PlanId = PlanId,
            Provider = PaymentProvider.Momo
        });

        first.Currency.Should().Be("VND");
        second.Currency.Should().Be("VND");
        first.OrderReference.Should().NotBe(second.OrderReference);
        db.Payments.Should().HaveCount(2);
    }

    #endregion

    #region U-P6 — refund rules

    [Fact]
    public async Task CreateRefundRequestAsync_NonCompletedPayment_Throws()
    {
        var db = TestPaymentDbContext.Create();
        var user = Guid.NewGuid();
        var payment = SeedPayment(db, user, status: PaymentStatus.Pending);

        var sut = Build(db);
        var act = () => sut.CreateRefundRequestAsync(user, payment.Id, "Changed my mind");

        await act.Should().ThrowAsync<InvalidRefundException>()
            .Where(e => e.Message.Contains("Only completed payments"));
    }

    [Fact]
    public async Task CreateRefundRequestAsync_OwnershipEnforced_Throws403()
    {
        var db = TestPaymentDbContext.Create();
        var owner = Guid.NewGuid();
        var other = Guid.NewGuid();
        var payment = SeedPayment(db, owner);

        var sut = Build(db);
        var act = () => sut.CreateRefundRequestAsync(other, payment.Id, "Not mine");

        await act.Should().ThrowAsync<InvalidRefundException>()
            .Where(e => e.StatusCode == 403);
    }

    [Fact]
    public async Task CreateRefundRequestAsync_UnknownPayment_Throws()
    {
        var db = TestPaymentDbContext.Create();

        var sut = Build(db);
        var act = () => sut.CreateRefundRequestAsync(Guid.NewGuid(), Guid.NewGuid(), "Lost");

        await act.Should().ThrowAsync<PaymentNotFoundException>();
    }

    [Fact]
    public async Task CreateRefundRequestAsync_CompletedPayment_CreatesPendingRequest()
    {
        var db = TestPaymentDbContext.Create();
        var user = Guid.NewGuid();
        var payment = SeedPayment(db, user);

        var sut = Build(db);
        var result = await sut.CreateRefundRequestAsync(user, payment.Id, "Changed my mind");

        result.Status.Should().Be(RefundStatus.Pending);
        result.PaymentOrderId.Should().Be(payment.Id);
        db.RefundRequests.Should().ContainSingle(r => r.UserId == user && r.Status == RefundStatus.Pending);
    }

    [Fact]
    public async Task CreateRefundRequestAsync_DuplicatePendingRequest_RejectsSecond()
    {
        var db = TestPaymentDbContext.Create();
        var user = Guid.NewGuid();
        var payment = SeedPayment(db, user);

        var sut = Build(db);
        await sut.CreateRefundRequestAsync(user, payment.Id, "First");
        var act = () => sut.CreateRefundRequestAsync(user, payment.Id, "Second");

        await act.Should().ThrowAsync<InvalidRefundException>().Where(e => e.StatusCode == 409);
        db.RefundRequests.Should().ContainSingle();
    }

    #endregion

    #region U-P7 — ownership guards

    [Fact]
    public async Task GetPaymentStatusAsync_OwnPayment_ReturnsStatus()
    {
        var db = TestPaymentDbContext.Create();
        var user = Guid.NewGuid();
        var payment = SeedPayment(db, user);

        var sut = Build(db);
        var result = await sut.GetPaymentStatusAsync(user, payment.Id);

        result.PaymentId.Should().Be(payment.Id);
        result.Status.Should().Be(payment.Status);
    }

    [Fact]
    public async Task GetPaymentReceiptAsync_CompletedOwnPayment_ReturnsConfirmation()
    {
        var db = TestPaymentDbContext.Create();
        var user = Guid.NewGuid();
        var payment = SeedPayment(db, user);
        payment.PlanId = PlanId;
        db.SaveChanges();

        var receipt = await Build(db).GetPaymentReceiptAsync(user, payment.Id);

        receipt.OrderReference.Should().Be(payment.OrderReference);
        receipt.Amount.Should().Be(payment.Amount);
        receipt.PlanId.Should().Be(PlanId);
    }

    [Fact]
    public async Task GetPaymentReceiptAsync_PendingOrOtherUser_IsRejected()
    {
        var db = TestPaymentDbContext.Create();
        var user = Guid.NewGuid();
        var payment = SeedPayment(db, user, PaymentStatus.Pending);
        payment.PlanId = PlanId;
        db.SaveChanges();
        var sut = Build(db);

        Func<Task> pending = () => sut.GetPaymentReceiptAsync(user, payment.Id);
        Func<Task> otherUser = () => sut.GetPaymentReceiptAsync(Guid.NewGuid(), payment.Id);
        await pending.Should().ThrowAsync<InvalidRefundException>().Where(e => e.StatusCode == 409);
        await otherUser.Should().ThrowAsync<InvalidRefundException>().Where(e => e.StatusCode == 403);
    }

    [Fact]
    public async Task GetPaymentStatusAsync_OtherUserPayment_Throws403()
    {
        var db = TestPaymentDbContext.Create();
        var payment = SeedPayment(db, Guid.NewGuid());

        var sut = Build(db);
        var act = () => sut.GetPaymentStatusAsync(Guid.NewGuid(), payment.Id);

        await act.Should().ThrowAsync<InvalidRefundException>()
            .Where(e => e.StatusCode == 403 && e.Message.Contains("does not belong"));
    }

    [Fact]
    public async Task GetPaymentHistoryAsync_ReturnsOnlyOwnPaymentsDescending()
    {
        var db = TestPaymentDbContext.Create();
        var user = Guid.NewGuid();
        var other = Guid.NewGuid();

        var newest = SeedPayment(db, user);
        new PaymentOrder
        {
            Id = Guid.NewGuid(),
            UserId = other,
            Amount = 1m,
            OrderReference = "PAY-OTHER",
            CreatedAt = DateTime.UtcNow.AddMinutes(-60),
            UpdatedAt = DateTime.UtcNow.AddMinutes(-60)
        }.ApplyTo(db);
        var oldest = SeedPayment(db, user);
        // make ordering deterministic: set explicit timestamps
        oldest.CreatedAt = DateTime.UtcNow.AddDays(-2);
        db.SaveChanges();

        var sut = Build(db);
        var history = await sut.GetPaymentHistoryAsync(user);

        history.Select(h => h.Id).Should().Equal(newest.Id, oldest.Id);
        history.Should().OnlyContain(h => h.Id == newest.Id || h.Id == oldest.Id);
    }

    #endregion

    private sealed class Valid<T> : AbstractValidator<T>
    {
    }
}

internal static class Extensions
{
    public static void ApplyTo(this PaymentOrder payment, TestPaymentDbContext db)
    {
        db.Payments.Add(payment);
        db.SaveChanges();
    }
}
