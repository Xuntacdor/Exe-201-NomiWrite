using NomiWrite.Writing.Application.DTOs;

namespace NomiWrite.Writing.Application.Interfaces;

public interface IAdminSubmissionService
{
    Task<SubmissionAnalyticsDto> GetAnalyticsAsync();
}