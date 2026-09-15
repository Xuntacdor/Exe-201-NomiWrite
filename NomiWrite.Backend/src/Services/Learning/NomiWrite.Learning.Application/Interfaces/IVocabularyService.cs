using NomiWrite.Learning.Application.DTOs;

namespace NomiWrite.Learning.Application.Interfaces;

public interface IVocabularyService
{
    Task<VocabularyPageDto> GetAllAsync(
        Guid userId,
        string? topic,
        bool? isMastered,
        int page,
        int pageSize);

    Task<VocabDto> UpdateMasteredAsync(Guid userId, Guid id, bool isMastered);
}