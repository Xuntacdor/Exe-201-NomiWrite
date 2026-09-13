using Microsoft.EntityFrameworkCore;
using NomiWrite.Payment.Application.Interfaces;
using NomiWrite.Payment.Domain.Entities;

namespace NomiWrite.Payment.Application.UnitTests.Persistence;

public sealed class TestPaymentDbContext : DbContext, IPaymentDbContext
{
    public TestPaymentDbContext(DbContextOptions<TestPaymentDbContext> options)
        : base(options)
    {
    }

    public DbSet<PaymentOrder> Payments => Set<PaymentOrder>();
    public DbSet<PaymentTransaction> PaymentTransactions => Set<PaymentTransaction>();
    public DbSet<RefundRequest> RefundRequests => Set<RefundRequest>();

    public static TestPaymentDbContext Create()
    {
        var options = new DbContextOptionsBuilder<TestPaymentDbContext>()
            .UseInMemoryDatabase($"test-payment-{Guid.NewGuid():N}")
            .Options;
        return new TestPaymentDbContext(options);
    }
}