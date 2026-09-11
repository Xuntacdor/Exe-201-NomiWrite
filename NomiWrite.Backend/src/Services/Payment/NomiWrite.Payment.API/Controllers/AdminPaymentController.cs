using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NomiWrite.Payment.Application.DTOs;
using NomiWrite.Payment.Application.Interfaces;

namespace NomiWrite.Payment.API.Controllers;

[ApiController]
[Route("api/admin/payments")]
[Authorize(Roles = "Admin")]
public class AdminPaymentController : ControllerBase
{
    private readonly IAdminPaymentService _adminPaymentService;

    public AdminPaymentController(IAdminPaymentService adminPaymentService)
        => _adminPaymentService = adminPaymentService;

    [HttpGet]
    public async Task<ActionResult<AdminPaymentListResponseDto>> GetPayments(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] string? provider = null,
        [FromQuery] string? status = null)
    {
        var query = new AdminPaymentQueryParamsDto
        {
            Page = page,
            PageSize = pageSize,
            FromDate = fromDate,
            ToDate = toDate
        };

        if (!string.IsNullOrWhiteSpace(provider)
            && Enum.TryParse<Domain.Enums.PaymentProvider>(provider, ignoreCase: true, out var parsedProvider))
        {
            query.Provider = parsedProvider;
        }

        if (!string.IsNullOrWhiteSpace(status)
            && Enum.TryParse<Domain.Enums.PaymentStatus>(status, ignoreCase: true, out var parsedStatus))
        {
            query.Status = parsedStatus;
        }

        var result = await _adminPaymentService.GetPaymentsAsync(query);
        return Ok(result);
    }

    [HttpGet("analytics")]
    public async Task<ActionResult<PaymentAnalyticsDto>> GetAnalytics()
    {
        var result = await _adminPaymentService.GetAnalyticsAsync();
        return Ok(result);
    }

    [HttpGet("export/csv")]
    public async Task<IActionResult> ExportCsv(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] string? provider = null,
        [FromQuery] string? status = null)
    {
        var query = new AdminPaymentQueryParamsDto
        {
            FromDate = fromDate,
            ToDate = toDate
        };

        if (!string.IsNullOrWhiteSpace(provider)
            && Enum.TryParse<Domain.Enums.PaymentProvider>(provider, ignoreCase: true, out var parsedProvider))
        {
            query.Provider = parsedProvider;
        }

        if (!string.IsNullOrWhiteSpace(status)
            && Enum.TryParse<Domain.Enums.PaymentStatus>(status, ignoreCase: true, out var parsedStatus))
        {
            query.Status = parsedStatus;
        }

        var bytes = await _adminPaymentService.ExportPaymentsCsvAsync(query);
        return File(
            bytes,
            "text/csv",
            $"payments_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv");
    }
}