using System.Text;
using FluentAssertions;
using NomiWrite.Payment.Application.DTOs;
using NomiWrite.Payment.Application.Services;
using NomiWrite.Payment.Application.UnitTests.Persistence;
using NomiWrite.Payment.Domain.Entities;
using NomiWrite.Payment.Domain.Enums;

namespace NomiWrite.Payment.Application.UnitTests;

public class AdminPaymentServiceTests
{
    private static PaymentOrder Seed(TestPaymentDbContext db, decimal amount, PaymentStatus status,
        PaymentProvider provider = PaymentProvider.VNPay, DateTime? createdAt = null)
    {
        var payment = new PaymentOrder
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Amount = amount,
            Currency = "VND",
            Provider = provider,
            Status = status,
            OrderReference = $"PAY-{Guid.NewGuid():N}".ToUpperInvariant(),
            AppliedDiscountPercent = status == PaymentStatus.Completed ? 10 : null,
            CreatedAt = createdAt ?? DateTime.UtcNow,
            UpdatedAt = createdAt ?? DateTime.UtcNow
        };
        db.Payments.Add(payment);
        db.SaveChanges();
        return payment;
    }

    #region U-P8 — CSV export + analytics

    [Fact]
    public async Task ExportPaymentsCsvAsync_WritesHeaderAndRows()
    {
        var db = TestPaymentDbContext.Create();
        var from = DateTime.UtcNow.AddDays(-1);
        var payment = Seed(db, 150_000m, PaymentStatus.Completed, createdAt: from);

        var sut = new AdminPaymentService(db);
        var bytes = await sut.ExportPaymentsCsvAsync(new AdminPaymentQueryParamsDto());
        var csv = Encoding.UTF8.GetString(bytes);

        var lines = csv.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        lines[0].Should().Be("Id,UserId,Amount,Currency,Provider,Status,PlanId,AppliedDiscountPercent,CreatedAt");
        lines.Should().ContainSingle(l => l.Contains(payment.Id.ToString()));
        lines[1].Should().Contain("150000");
        lines[1].Should().Contain("VND");
        lines[1].Should().Contain("10");
    }

    [Fact]
    public async Task ExportPaymentsCsvAsync_RespectsFilters()
    {
        var db = TestPaymentDbContext.Create();
        Seed(db, 50_000m, PaymentStatus.Completed, provider: PaymentProvider.VNPay);
        Seed(db, 70_000m, PaymentStatus.Failed, provider: PaymentProvider.Momo);

        var sut = new AdminPaymentService(db);
        var bytes = await sut.ExportPaymentsCsvAsync(new AdminPaymentQueryParamsDto
        {
            Provider = PaymentProvider.Momo,
            Status = PaymentStatus.Failed
        });
        var csv = Encoding.UTF8.GetString(bytes);

        csv.Split('\n', StringSplitOptions.RemoveEmptyEntries).Should().HaveCount(2);
        csv.Should().Contain("70000");
        csv.Should().NotContain("50000");
    }

    [Fact]
    public async Task GetAnalyticsAsync_SumsCompletedOnly()
    {
        var db = TestPaymentDbContext.Create();
        Seed(db, 100_000m, PaymentStatus.Completed);
        Seed(db, 200_000m, PaymentStatus.Completed);
        Seed(db, 999_999m, PaymentStatus.Pending);
        Seed(db, 888_888m, PaymentStatus.Failed);

        var sut = new AdminPaymentService(db);
        var analytics = await sut.GetAnalyticsAsync();

        analytics.TotalRevenue.Should().Be(300_000m);
        analytics.CompletedTransactions.Should().Be(2);
    }

    [Fact]
    public async Task GetPaymentsAsync_Paginates()
    {
        var db = TestPaymentDbContext.Create();
        for (var i = 0; i < 5; i++)
            Seed(db, 10_000m + i, PaymentStatus.Completed);

        var sut = new AdminPaymentService(db);
        var first = await sut.GetPaymentsAsync(new AdminPaymentQueryParamsDto { Page = 1, PageSize = 2 });
        var second = await sut.GetPaymentsAsync(new AdminPaymentQueryParamsDto { Page = 2, PageSize = 2 });

        first.Items.Should().HaveCount(2);
        second.Items.Should().HaveCount(2);
        first.Page.Should().Be(1);
        second.Page.Should().Be(2);
        first.TotalCount.Should().Be(5);
        first.TotalPages.Should().Be(3);
        first.Items.Select(i => i.Id).Should().NotIntersectWith(second.Items.Select(i => i.Id));
    }

    #endregion
}