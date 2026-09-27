using NomiWrite.Learning.Application.DTOs;

namespace NomiWrite.Learning.Application.Interfaces;

public interface IVocabularyService
{
    Task<VocabularyPageDto> GetAllAsync(
        Guid userId,
        Guid? submissionId,
        string? topic,
        bool? isMastered,
        int page,
        int pageSize);

    Task<VocabDto> UpdateMasteredAsync(Guid userId, Guid id, bool isMastered);
    Task<VocabGroupDto> CreateGroupAsync(Guid userId, CreateVocabGroupRequestDto request);
    Task<List<VocabGroupDto>> ListGroupsAsync(Guid userId);
    Task AddToGroupAsync(Guid userId, Guid groupId, AddToVocabGroupRequestDto request);
    Task RemoveFromGroupAsync(Guid userId, Guid groupId, Guid vocabId);
}