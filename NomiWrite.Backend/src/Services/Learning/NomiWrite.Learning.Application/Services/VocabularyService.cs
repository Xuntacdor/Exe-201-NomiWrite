using Microsoft.EntityFrameworkCore;
using NomiWrite.Learning.Application.DTOs;
using NomiWrite.Learning.Application.Exceptions;
using NomiWrite.Learning.Application.Interfaces;
using NomiWrite.Learning.Domain.Entities;

namespace NomiWrite.Learning.Application.Services;

public class VocabularyService : IVocabularyService
{
    private readonly ILearningDbContext _dbContext;

    public VocabularyService(ILearningDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<VocabularyPageDto> GetAllAsync(
        Guid userId,
        string? topic,
        bool? isMastered,
        int page,
        int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = _dbContext.VocabSuggestions
            .AsNoTracking()
            .Where(v => v.UserId == userId);

        if (!string.IsNullOrWhiteSpace(topic))
        {
            var normalizedTopic = topic.Trim().ToLowerInvariant();
            query = query.Where(v => v.Topic.ToLower() == normalizedTopic);
        }

        if (isMastered.HasValue)
            query = query.Where(v => v.IsMastered == isMastered.Value);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(v => v.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(v => new VocabDto
            {
                Id = v.Id,
                UserId = v.UserId,
                SubmissionId = v.SubmissionId,
                Topic = v.Topic,
                OriginalWord = v.OriginalWord,
                SuggestedWord = v.SuggestedWord,
                ExampleSentence = v.ExampleSentence,
                IsMastered = v.IsMastered,
                CreatedAt = v.CreatedAt
            })
            .ToListAsync();

        var totalPages = pageSize > 0 ? (int)Math.Ceiling(totalCount / (double)pageSize) : 0;

        return new VocabularyPageDto
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = totalPages
        };
    }

    public async Task<VocabDto> UpdateMasteredAsync(Guid userId, Guid id, bool isMastered)
    {
        var suggestion = await _dbContext.VocabSuggestions
            .FirstOrDefaultAsync(v => v.Id == id);

        if (suggestion is null)
            throw new VocabSuggestionNotFoundException(id);

        if (suggestion.UserId != userId)
            throw new ForbiddenLearningAccessException();

        suggestion.IsMastered = isMastered;
        suggestion.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return new VocabDto
        {
            Id = suggestion.Id,
            UserId = suggestion.UserId,
            SubmissionId = suggestion.SubmissionId,
            Topic = suggestion.Topic,
            OriginalWord = suggestion.OriginalWord,
            SuggestedWord = suggestion.SuggestedWord,
            ExampleSentence = suggestion.ExampleSentence,
            IsMastered = suggestion.IsMastered,
            CreatedAt = suggestion.CreatedAt
        };
    }
}