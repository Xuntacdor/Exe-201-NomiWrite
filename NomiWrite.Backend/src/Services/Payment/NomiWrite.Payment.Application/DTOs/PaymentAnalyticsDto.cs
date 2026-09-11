namespace NomiWrite.Payment.Application.DTOs;

public class PaymentAnalyticsDto
{
    public decimal TotalRevenue { get; set; }
    public int CompletedTransactions { get; set; }
}