using NomiWrite.Subscription.Application.DTOs;

namespace NomiWrite.Subscription.Application.Interfaces;

public interface IAdminPlanService
{
    Task<IReadOnlyList<AdminPlanListItemDto>> GetPlansAsync();

    Task<AdminPlanListItemDto> CreatePlanAsync(CreatePlanRequestDto request);

    Task<AdminPlanListItemDto> UpdatePlanAsync(Guid id, UpdatePlanRequestDto request);

    Task<AdminPlanListItemDto> UpdatePlanStatusAsync(Guid id, bool isActive);
}