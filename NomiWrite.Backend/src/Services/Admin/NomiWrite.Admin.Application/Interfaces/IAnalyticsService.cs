using NomiWrite.Admin.Application.DTOs;

namespace NomiWrite.Admin.Application.Interfaces;

public interface IAnalyticsService
{
    Task<AnalyticsOverviewDto> GetOverviewAsync();
}
