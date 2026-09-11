using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore;
using NomiWrite.Payment.Application.DTOs;
using NomiWrite.Payment.Application.Interfaces;
using NomiWrite.Payment.Domain.Enums;

namespace NomiWrite.Payment.Application.Services;

public class AdminPaymentService : IAdminPaymentService
{
    private readonly IPaymentDbContext _dbContext;

    public AdminPaymentService(IPaymentDbContext dbContext)
        => _dbContext = dbContext;

    public async Task<AdminPaymentListResponseDto> GetPaymentsAsync(AdminPaymentQueryParamsDto query)
    {
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize is < 1 or > 100 ? 20 : query.PageSize;

        var request = ApplyFilters(query);

        var totalCount = await request.CountAsync();
        var items = await request
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new AdminPaymentItemDto
            {
                Id = p.Id,
                UserId = p.UserId,
                Amount = p.Amount,
                Currency = p.Currency,
                Provider = p.Provider,
                Status = p.Status,
                PlanId = p.PlanId,
                AppliedDiscountPercent = p.AppliedDiscountPercent,
                OrderReference = p.OrderReference,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            })
            .ToListAsync();

        return new AdminPaymentListResponseDto
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
            Items = items
        };
    }

    public async Task<byte[]> ExportPaymentsCsvAsync(AdminPaymentQueryParamsDto query)
    {
        var payments = await ApplyFilters(query)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new AdminPaymentItemDto
            {
                Id = p.Id,
                UserId = p.UserId,
                Amount = p.Amount,
                Currency = p.Currency,
                Provider = p.Provider,
                Status = p.Status,
                PlanId = p.PlanId,
                AppliedDiscountPercent = p.AppliedDiscountPercent,
                OrderReference = p.OrderReference,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            })
            .ToListAsync();

        var csv = new StringBuilder();
        csv.AppendLine("Id,UserId,Amount,Currency,Provider,Status,PlanId,AppliedDiscountPercent,CreatedAt");

        foreach (var p in payments)
        {
            csv.AppendLine(string.Join(",",
                p.Id,
                p.UserId,
                p.Amount.ToString(CultureInfo.InvariantCulture),
                CsvEscape(p.Currency),
                p.Provider,
                p.Status,
                p.PlanId?.ToString() ?? string.Empty,
                p.AppliedDiscountPercent?.ToString(CultureInfo.InvariantCulture) ?? string.Empty,
                p.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)));
        }

        return Encoding.UTF8.GetBytes(csv.ToString());
    }

    public async Task<PaymentAnalyticsDto> GetAnalyticsAsync()
    {
        var completedPayments = _dbContext.Payments
            .AsNoTracking()
            .Where(p => p.Status == PaymentStatus.Completed);

        return new PaymentAnalyticsDto
        {
            TotalRevenue = await completedPayments.SumAsync(p => (decimal?)p.Amount) ?? 0m,
            CompletedTransactions = await completedPayments.CountAsync()
        };
    }

    private IQueryable<Domain.Entities.PaymentOrder> ApplyFilters(AdminPaymentQueryParamsDto query)
    {
        var request = _dbContext.Payments.AsNoTracking();

        if (query.FromDate.HasValue)
            request = request.Where(p => p.CreatedAt >= query.FromDate.Value);

        if (query.ToDate.HasValue)
            request = request.Where(p => p.CreatedAt < query.ToDate.Value.AddDays(1));

        if (query.Provider.HasValue)
            request = request.Where(p => p.Provider == query.Provider.Value);

        if (query.Status.HasValue)
            request = request.Where(p => p.Status == query.Status.Value);

        return request;
    }

    private static string CsvEscape(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return value;
    }
}