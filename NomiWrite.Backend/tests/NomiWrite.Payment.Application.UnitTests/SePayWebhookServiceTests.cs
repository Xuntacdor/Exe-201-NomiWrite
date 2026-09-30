using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NomiWrite.Payment.Application.DTOs;
using NomiWrite.Payment.Application.Interfaces;
using NomiWrite.Payment.Application.Services;
using NomiWrite.Payment.Application.UnitTests.Persistence;
using NomiWrite.Payment.Domain.Entities;
using NomiWrite.Payment.Domain.Enums;
using NomiWrite.Shared.Contracts.Events.Payment;

namespace NomiWrite.Payment.Application.UnitTests;

public class SePayWebhookServiceTests
{
    private const string AccountNumber = "1234567890";

    private static readonly Guid PlanId = Guid.NewGuid();

    private static SePayWebhookService Build(TestPaymentDbContext db, IPublishEndpoint publish)
        => new(db, publish, NullLogger<SePayWebhookService>.Instance);

    private static PaymentOrder SeedVietQrOrder(
        TestPaymentDbContext db,
        out Guid userId,
        PaymentStatus status = PaymentStatus.Pending,
        decimal amount = 100_000m,
        string? orderReference = null)
    {
        userId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        var payment = new PaymentOrder
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Amount = amount,
            Currency = "VND",
            Provider = PaymentProvider.VietQR,
            Status = status,
            OrderReference = orderReference ?? SePayOrderReference.Create(),
            PlanId = PlanId,
            CreatedAt = now,
            UpdatedAt = now
        };

        db.Payments.Add(payment);
        db.SaveChanges();
        return payment;
    }

    private static SePayWebhookDto Transfer(
        PaymentOrder payment,
        long amount = 100_000,
        string? code = null,
        string? content = null,
        string transferType = "in",
        long id = 92704)
        => new()
        {
            Id = id,
            Gateway = "Vietcombank",
            TransactionDate = "2024-07-02 11:08:33",
            AccountNumber = AccountNumber,
            SubAccount = string.Empty,
            Code = code ?? payment.OrderReference,
            Content = content ?? $"{payment.OrderReference} chuyen tien",
            TransferType = transferType,
            Description = "NGUYEN VAN A chuyen tien",
            TransferAmount = amount,
            Accumulated = 105_000_000,
            ReferenceCode = "FT24012345678"
        };

    private static IReadOnlyList<PaymentCompletedEvent> CompletedEvents(IPublishEndpoint publish)
        => publish.ReceivedCalls()
            .SelectMany(c => c.GetArguments())
            .OfType<PaymentCompletedEvent>()
            .ToList();

    // ── Fulfillment ────────────────────────────────────────────────────────────

    [Fact]
    public async Task ProcessAsync_MatchingTransfer_CompletesOrderAndPublishes()
    {
        var db = TestPaymentDbContext.Create();
        var publish = Substitute.For<IPublishEndpoint>();
        var order = SeedVietQrOrder(db, out var userId);

        var result = await Build(db, publish).ProcessAsync(Transfer(order), AccountNumber);

        result.Outcome.Should().Be(SePayWebhookOutcome.Completed);
        result.OrderReference.Should().Be(order.OrderReference);

        var settled = db.Payments.Single(p => p.Id == order.Id);
        settled.Status.Should().Be(PaymentStatus.Completed);
        settled.UpdatedAt.Should().NotBeNull();

        var completed = CompletedEvents(publish).Should().ContainSingle().Subject;
        completed.PaymentId.Should().Be(order.Id);
        completed.UserId.Should().Be(userId);
        completed.PlanId.Should().Be(PlanId);
    }

    [Fact]
    public async Task ProcessAsync_MatchingTransfer_RecordsRawPayloadForReconciliation()
    {
        var db = TestPaymentDbContext.Create();
        var publish = Substitute.For<IPublishEndpoint>();
        var order = SeedVietQrOrder(db, out _);

        await Build(db, publish).ProcessAsync(Transfer(order), AccountNumber);

        var transaction = db.PaymentTransactions.Should().ContainSingle().Subject;
        transaction.PaymentId.Should().Be(order.Id);
        transaction.ProviderTransactionId.Should().Be("92704");
        transaction.RawPayload.Should().Contain("FT24012345678");
        transaction.RawPayload.Should().Contain("Vietcombank");
    }

    [Fact]
    public async Task ProcessAsync_Overpayment_StillCompletesTheOrder()
    {
        var db = TestPaymentDbContext.Create();
        var publish = Substitute.For<IPublishEndpoint>();
        var order = SeedVietQrOrder(db, out _, amount: 199_000m);

        var result = await Build(db, publish).ProcessAsync(Transfer(order, amount: 250_000), AccountNumber);

        result.Outcome.Should().Be(SePayWebhookOutcome.Completed);
        db.Payments.Single(p => p.Id == order.Id).Status.Should().Be(PaymentStatus.Completed);
    }

    // ── Order matching ─────────────────────────────────────────────────────────

    [Fact]
    public async Task ProcessAsync_ReferenceInContentOnly_StillMatches()
    {
        var db = TestPaymentDbContext.Create();
        var publish = Substitute.For<IPublishEndpoint>();
        var order = SeedVietQrOrder(db, out _);

        // SePay only fills `code` when the transfer content matches the pattern
        // configured in its dashboard, so a hand-typed reference arrives without it.
        var payload = Transfer(order, code: null, content: $"thanh toan {order.OrderReference} NomiWrite");

        var result = await Build(db, publish).ProcessAsync(payload, AccountNumber);

        result.Outcome.Should().Be(SePayWebhookOutcome.Completed);
        db.Payments.Single(p => p.Id == order.Id).Status.Should().Be(PaymentStatus.Completed);
    }

    [Fact]
    public async Task ProcessAsync_LowercaseReference_IsNormalised()
    {
        var db = TestPaymentDbContext.Create();
        var publish = Substitute.For<IPublishEndpoint>();
        var order = SeedVietQrOrder(db, out _);

        var payload = Transfer(order, code: order.OrderReference.ToLowerInvariant());

        var result = await Build(db, publish).ProcessAsync(payload, AccountNumber);

        result.Outcome.Should().Be(SePayWebhookOutcome.Completed);
    }

    [Fact]
    public async Task ProcessAsync_UnknownReference_IsIgnoredWithoutPublishing()
    {
        var db = TestPaymentDbContext.Create();
        var publish = Substitute.For<IPublishEndpoint>();
        SeedVietQrOrder(db, out _);

        var payload = new SePayWebhookDto
        {
            Id = 1,
            AccountNumber = AccountNumber,
            Code = SePayOrderReference.Create(),
            Content = "khong lien quan",
            TransferType = "in",
            TransferAmount = 100_000
        };

        var result = await Build(db, publish).ProcessAsync(payload, AccountNumber);

        result.Outcome.Should().Be(SePayWebhookOutcome.Ignored);
        result.Reason.Should().Be("order_not_found");
        CompletedEvents(publish).Should().BeEmpty();
    }

    [Fact]
    public async Task ProcessAsync_TransferWithoutAnyReference_IsIgnored()
    {
        var db = TestPaymentDbContext.Create();
        var publish = Substitute.For<IPublishEndpoint>();
        SeedVietQrOrder(db, out _);

        var payload = new SePayWebhookDto
        {
            Id = 2,
            AccountNumber = AccountNumber,
            Content = "chuyen tien sinh hoat",
            TransferType = "in",
            TransferAmount = 100_000
        };

        var result = await Build(db, publish).ProcessAsync(payload, AccountNumber);

        result.Reason.Should().Be("no_order_reference");
        CompletedEvents(publish).Should().BeEmpty();
    }

    [Fact]
    public async Task ProcessAsync_OrderBelongingToAnotherProvider_IsIgnored()
    {
        var db = TestPaymentDbContext.Create();
        var publish = Substitute.For<IPublishEndpoint>();

        var now = DateTime.UtcNow;
        var order = new PaymentOrder
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Amount = 100_000m,
            Currency = "VND",
            Provider = PaymentProvider.VNPay,
            Status = PaymentStatus.Pending,
            OrderReference = SePayOrderReference.Create(),
            CreatedAt = now,
            UpdatedAt = now
        };
        db.Payments.Add(order);
        db.SaveChanges();

        var result = await Build(db, publish).ProcessAsync(Transfer(order), AccountNumber);

        result.Outcome.Should().Be(SePayWebhookOutcome.Ignored);
        result.Reason.Should().Be("provider_mismatch");
        db.Payments.Single().Status.Should().Be(PaymentStatus.Pending);
    }

    // ── Validation ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task ProcessAsync_OutgoingTransfer_NeverSettlesAnOrder()
    {
        var db = TestPaymentDbContext.Create();
        var publish = Substitute.For<IPublishEndpoint>();
        var order = SeedVietQrOrder(db, out _);

        var result = await Build(db, publish)
            .ProcessAsync(Transfer(order, transferType: "out"), AccountNumber);

        result.Reason.Should().Be("transfer_type");
        db.Payments.Single(p => p.Id == order.Id).Status.Should().Be(PaymentStatus.Pending);
        CompletedEvents(publish).Should().BeEmpty();
    }

    [Fact]
    public async Task ProcessAsync_Underpayment_DoesNotSettleTheOrder()
    {
        var db = TestPaymentDbContext.Create();
        var publish = Substitute.For<IPublishEndpoint>();
        var order = SeedVietQrOrder(db, out _, amount: 199_000m);

        var result = await Build(db, publish)
            .ProcessAsync(Transfer(order, amount: 199_000 - 1), AccountNumber);

        result.Outcome.Should().Be(SePayWebhookOutcome.Ignored);
        result.Reason.Should().Be("amount_below_order_total");
        db.Payments.Single(p => p.Id == order.Id).Status.Should().Be(PaymentStatus.Pending);
        CompletedEvents(publish).Should().BeEmpty();
    }

    [Fact]
    public async Task ProcessAsync_TransferToAnotherAccount_IsIgnored()
    {
        var db = TestPaymentDbContext.Create();
        var publish = Substitute.For<IPublishEndpoint>();
        var order = SeedVietQrOrder(db, out _);

        var result = await Build(db, publish).ProcessAsync(Transfer(order), "9999999999");

        result.Reason.Should().Be("unexpected_account");
        db.Payments.Single(p => p.Id == order.Id).Status.Should().Be(PaymentStatus.Pending);
    }

    [Fact]
    public async Task ProcessAsync_NullExpectedAccount_SkipsTheAccountCheck()
    {
        var db = TestPaymentDbContext.Create();
        var publish = Substitute.For<IPublishEndpoint>();
        var order = SeedVietQrOrder(db, out _);

        var result = await Build(db, publish).ProcessAsync(Transfer(order), expectedAccountNumber: null);

        result.Outcome.Should().Be(SePayWebhookOutcome.Completed);
    }

    // ── Idempotency ────────────────────────────────────────────────────────────

    [Fact]
    public async Task ProcessAsync_ReplayedTransfer_DoesNotFulfilTwice()
    {
        var db = TestPaymentDbContext.Create();
        var publish = Substitute.For<IPublishEndpoint>();
        var order = SeedVietQrOrder(db, out _);
        var payload = Transfer(order);
        var sut = Build(db, publish);

        (await sut.ProcessAsync(payload, AccountNumber)).Outcome.Should().Be(SePayWebhookOutcome.Completed);

        // SePay replays a transfer on retry and when an admin replays it by hand.
        var replay = await sut.ProcessAsync(payload, AccountNumber);

        replay.Outcome.Should().Be(SePayWebhookOutcome.Duplicate);
        db.PaymentTransactions.Should().ContainSingle();
        CompletedEvents(publish).Should().ContainSingle();
    }

    [Fact]
    public async Task ProcessAsync_SecondTransferForSameOrder_IsDuplicate()
    {
        var db = TestPaymentDbContext.Create();
        var publish = Substitute.For<IPublishEndpoint>();
        var order = SeedVietQrOrder(db, out _);
        var sut = Build(db, publish);

        await sut.ProcessAsync(Transfer(order, id: 1), AccountNumber);

        // A different SePay id carrying the same order reference, e.g. the customer
        // paid twice. The first transfer already settled the order.
        var second = await sut.ProcessAsync(Transfer(order, id: 2), AccountNumber);

        second.Outcome.Should().Be(SePayWebhookOutcome.Duplicate);
        second.Reason.Should().Be("order_already_Completed");
        CompletedEvents(publish).Should().ContainSingle();
    }

    [Fact]
    public async Task ProcessAsync_TransferRecordedAgainstAnotherOrder_IsDuplicate()
    {
        var db = TestPaymentDbContext.Create();
        var publish = Substitute.For<IPublishEndpoint>();
        var first = SeedVietQrOrder(db, out _);
        var second = SeedVietQrOrder(db, out _);

        await Build(db, publish).ProcessAsync(Transfer(first, id: 5), AccountNumber);

        // Same SePay transaction id replayed against a different order reference.
        var replay = await Build(db, publish).ProcessAsync(Transfer(second, id: 5), AccountNumber);

        replay.Outcome.Should().Be(SePayWebhookOutcome.Duplicate);
        replay.Reason.Should().Be("transaction_already_recorded");
        db.Payments.Single(p => p.Id == second.Id).Status.Should().Be(PaymentStatus.Pending);
    }

    [Fact]
    public async Task ProcessAsync_AlreadyCompletedOrder_IsDuplicate()
    {
        var db = TestPaymentDbContext.Create();
        var publish = Substitute.For<IPublishEndpoint>();
        var order = SeedVietQrOrder(db, out _, status: PaymentStatus.Completed);

        var result = await Build(db, publish).ProcessAsync(Transfer(order), AccountNumber);

        result.Outcome.Should().Be(SePayWebhookOutcome.Duplicate);
        result.Reason.Should().Be("order_already_Completed");
        CompletedEvents(publish).Should().BeEmpty();
    }

    [Fact]
    public async Task ProcessAsync_OrderWithExistingTransaction_IsDuplicate()
    {
        var db = TestPaymentDbContext.Create();
        var publish = Substitute.For<IPublishEndpoint>();
        var order = SeedVietQrOrder(db, out _);

        // Mirrors ix_payment_transactions_payment_id: an order holds one transfer.
        db.PaymentTransactions.Add(new PaymentTransaction
        {
            PaymentId = order.Id,
            ProviderTransactionId = "some-other-bank-id",
            RawPayload = "{}"
        });
        db.SaveChanges();

        var result = await Build(db, publish).ProcessAsync(Transfer(order), AccountNumber);

        result.Outcome.Should().Be(SePayWebhookOutcome.Duplicate);
        db.Payments.Single(p => p.Id == order.Id).Status.Should().Be(PaymentStatus.Pending);
    }

    [Fact]
    public async Task ProcessAsync_FailedOrder_IsNotReopened()
    {
        var db = TestPaymentDbContext.Create();
        var publish = Substitute.For<IPublishEndpoint>();
        var order = SeedVietQrOrder(db, out _, status: PaymentStatus.Failed);

        var result = await Build(db, publish).ProcessAsync(Transfer(order), AccountNumber);

        result.Outcome.Should().Be(SePayWebhookOutcome.Duplicate);
        db.Payments.Single(p => p.Id == order.Id).Status.Should().Be(PaymentStatus.Failed);
    }
}
