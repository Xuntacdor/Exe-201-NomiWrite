using Microsoft.EntityFrameworkCore;
using NomiWrite.Payment.Domain.Entities;

namespace NomiWrite.Payment.Application.Interfaces;

public interface IPaymentDbContext
{
    DbSet<PaymentOrder> Payments { get; }
    DbSet<PaymentTransaction> PaymentTransactions { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}