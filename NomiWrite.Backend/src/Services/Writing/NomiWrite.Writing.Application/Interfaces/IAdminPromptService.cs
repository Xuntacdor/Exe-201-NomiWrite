using NomiWrite.Writing.Application.DTOs;
using NomiWrite.Writing.Domain.Enums;

namespace NomiWrite.Writing.Application.Interfaces;

public interface IAdminPromptService
{
    Task<PagedResultDto<AdminPromptListItemDto>> GetPromptsAsync(
        Guid? typeId, DifficultyLevel? difficulty, bool? isActive, int page, int pageSize);

    Task<AdminPromptListItemDto> GetPromptByIdAsync(Guid id);

    Task<AdminPromptListItemDto> CreatePromptAsync(CreatePromptRequestDto request);

    Task<AdminPromptListItemDto> UpdatePromptAsync(Guid id, UpdatePromptRequestDto request);

    Task DeletePromptAsync(Guid id);
}