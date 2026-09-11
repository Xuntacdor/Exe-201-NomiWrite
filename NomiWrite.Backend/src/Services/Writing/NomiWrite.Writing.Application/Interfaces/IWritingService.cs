using NomiWrite.Writing.Application.DTOs;
using NomiWrite.Writing.Domain.Enums;

namespace NomiWrite.Writing.Application.Interfaces;

public interface IWritingService
{
    Task<IReadOnlyList<WritingTypeDto>> GetWritingTypesAsync();

    Task<IReadOnlyList<WritingPromptListItemDto>> GetPromptsAsync(
        Guid? typeId,
        DifficultyLevel? difficulty,
        bool random,
        Guid? userId = null,
        string? accessToken = null);

    Task<WritingPromptDto> GetPromptByIdAsync(Guid id);

    Task<SubmissionResponseDto> CreateSubmissionAsync(Guid userId, CreateSubmissionRequestDto dto);

    Task<SubmissionResponseDto> UpdateSubmissionAsync(Guid userId, Guid submissionId, UpdateSubmissionRequestDto dto);

    Task<SubmissionResponseDto> SubmitSubmissionAsync(Guid userId, Guid submissionId);

    Task<SubmissionResponseDto> GetSubmissionByIdAsync(Guid userId, Guid submissionId);

    Task<IReadOnlyList<SubmissionListItemDto>> GetUserSubmissionsAsync(Guid userId);

    Task<SubmissionTimeRemainingDto> GetSubmissionTimeRemainingAsync(Guid userId, Guid submissionId);

    Task<SampleAnswerDto> GetSampleAnswerAsync(Guid userId, Guid promptId, string? accessToken);
}