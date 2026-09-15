using NomiWrite.Payment.Application.DTOs;

namespace NomiWrite.Payment.Application.Interfaces;

public interface IAdminPaymentService
{
    Task<AdminPaymentListResponseDto> GetPaymentsAsync(AdminPaymentQueryParamsDto query);

    Task<byte[]> ExportPaymentsCsvAsync(AdminPaymentQueryParamsDto query);

    Task<PaymentAnalyticsDto> GetAnalyticsAsync();
}