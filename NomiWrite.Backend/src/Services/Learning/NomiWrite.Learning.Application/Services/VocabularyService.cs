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
        Guid? submissionId,
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

        if (submissionId.HasValue)
            query = query.Where(v => v.SubmissionId == submissionId.Value);

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

    public async Task<VocabGroupDto> CreateGroupAsync(Guid userId, CreateVocabGroupRequestDto request)
    {
        var group = new VocabGroup
        {
            UserId = userId,
            Name = request.Name
        };

        _dbContext.VocabGroups.Add(group);
        await _dbContext.SaveChangesAsync();

        if (request.VocabularyIds != null && request.VocabularyIds.Any())
        {
            await AddToGroupAsync(userId, group.Id, new AddToVocabGroupRequestDto { VocabularyIds = request.VocabularyIds });
        }

        return new VocabGroupDto
        {
            Id = group.Id,
            Name = group.Name,
            CreatedAt = group.CreatedAt,
            WordCount = request.VocabularyIds?.Count ?? 0,
            VocabularyIds = request.VocabularyIds ?? new List<Guid>()
        };
    }

    public async Task<List<VocabGroupDto>> ListGroupsAsync(Guid userId)
    {
        var groups = await _dbContext.VocabGroups
            .AsNoTracking()
            .Where(g => g.UserId == userId)
            .OrderByDescending(g => g.CreatedAt)
            .Include(g => g.Items)
            .Select(g => new VocabGroupDto
            {
                Id = g.Id,
                Name = g.Name,
                CreatedAt = g.CreatedAt,
                WordCount = g.Items.Count,
                VocabularyIds = g.Items.Select(i => i.VocabSuggestionId).ToList()
            })
            .ToListAsync();

        return groups;
    }

    public async Task AddToGroupAsync(Guid userId, Guid groupId, AddToVocabGroupRequestDto request)
    {
        var group = await _dbContext.VocabGroups
            .Include(g => g.Items)
            .FirstOrDefaultAsync(g => g.Id == groupId && g.UserId == userId);

        if (group == null)
            throw new Exception("Vocabulary group not found or access denied."); // Custom exception can be used

        var existingVocabIds = group.Items.Select(i => i.VocabSuggestionId).ToHashSet();
        
        foreach (var vocabId in request.VocabularyIds)
        {
            if (!existingVocabIds.Contains(vocabId))
            {
                group.Items.Add(new VocabGroupItem
                {
                    VocabGroupId = groupId,
                    VocabSuggestionId = vocabId
                });
            }
        }

        await _dbContext.SaveChangesAsync();
    }

    public async Task RemoveFromGroupAsync(Guid userId, Guid groupId, Guid vocabId)
    {
        var group = await _dbContext.VocabGroups
            .Include(g => g.Items)
            .FirstOrDefaultAsync(g => g.Id == groupId && g.UserId == userId);

        if (group == null)
            throw new Exception("Vocabulary group not found or access denied.");

        var item = group.Items.FirstOrDefault(i => i.VocabSuggestionId == vocabId);
        if (item != null)
        {
            group.Items.Remove(item);
            await _dbContext.SaveChangesAsync();
        }
    }
}