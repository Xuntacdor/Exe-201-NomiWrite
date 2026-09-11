using FluentValidation;
using Microsoft.EntityFrameworkCore;
using NomiWrite.Writing.Application.DTOs;
using NomiWrite.Writing.Application.Exceptions;
using NomiWrite.Writing.Application.Interfaces;
using NomiWrite.Writing.Domain.Entities;
using NomiWrite.Writing.Domain.Enums;

namespace NomiWrite.Writing.Application.Services;

public class AdminPromptService : IAdminPromptService
{
    private readonly IWritingDbContext _dbContext;
    private readonly IValidator<CreatePromptRequestDto> _createPromptValidator;
    private readonly IValidator<UpdatePromptRequestDto> _updatePromptValidator;

    public AdminPromptService(
        IWritingDbContext dbContext,
        IValidator<CreatePromptRequestDto> createPromptValidator,
        IValidator<UpdatePromptRequestDto> updatePromptValidator)
    {
        _dbContext = dbContext;
        _createPromptValidator = createPromptValidator;
        _updatePromptValidator = updatePromptValidator;
    }

    public async Task<PagedResultDto<AdminPromptListItemDto>> GetPromptsAsync(
        Guid? typeId, DifficultyLevel? difficulty, bool? isActive, int page, int pageSize)
    {
        var query = _dbContext.WritingPrompts
            .AsNoTracking()
            .AsQueryable();

        // By default return active prompts; pass isActive explicitly to see inactive ones.
        var activeFilter = isActive ?? true;
        query = query.Where(p => p.IsActive == activeFilter);

        if (typeId.HasValue)
            query = query.Where(p => p.WritingTypeId == typeId.Value);

        if (difficulty.HasValue)
            query = query.Where(p => p.Difficulty == difficulty.Value);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new AdminPromptListItemDto
            {
                Id = p.Id,
                WritingTypeId = p.WritingTypeId,
                Title = p.Title,
                Difficulty = p.Difficulty,
                IsActive = p.IsActive,
                TimeLimitMinutes = p.TimeLimitMinutes,
                MinWords = p.MinWords,
                MaxWords = p.MaxWords,
                ImageUrl = p.ImageUrl,
                IsVipOnly = p.IsVipOnly,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync();

        return new PagedResultDto<AdminPromptListItemDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }

    public async Task<AdminPromptListItemDto> GetPromptByIdAsync(Guid id)
    {
        var prompt = await _dbContext.WritingPrompts
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new PromptNotFoundException(id);

        return new AdminPromptListItemDto
        {
            Id = prompt.Id,
            WritingTypeId = prompt.WritingTypeId,
            Title = prompt.Title,
            Difficulty = prompt.Difficulty,
            IsActive = prompt.IsActive,
            TimeLimitMinutes = prompt.TimeLimitMinutes,
            MinWords = prompt.MinWords,
            MaxWords = prompt.MaxWords,
            ImageUrl = prompt.ImageUrl,
            IsVipOnly = prompt.IsVipOnly,
            CreatedAt = prompt.CreatedAt
        };
    }

    public async Task<AdminPromptListItemDto> CreatePromptAsync(CreatePromptRequestDto request)
    {
        var validationResult = await _createPromptValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var prompt = new WritingPrompt
        {
            WritingTypeId = request.WritingTypeId,
            Title = request.Title.Trim(),
            Instructions = request.Instructions.Trim(),
            Difficulty = request.Difficulty,
            IsActive = true,
            TimeLimitMinutes = request.TimeLimitMinutes,
            MinWords = request.MinWords,
            MaxWords = request.MaxWords,
            ImageUrl = request.ImageUrl?.Trim(),
            SampleAnswer = request.SampleAnswer?.Trim(),
            IsVipOnly = request.IsVipOnly
        };

        _dbContext.WritingPrompts.Add(prompt);
        await _dbContext.SaveChangesAsync();

        return new AdminPromptListItemDto
        {
            Id = prompt.Id,
            WritingTypeId = prompt.WritingTypeId,
            Title = prompt.Title,
            Difficulty = prompt.Difficulty,
            IsActive = prompt.IsActive,
            TimeLimitMinutes = prompt.TimeLimitMinutes,
            MinWords = prompt.MinWords,
            MaxWords = prompt.MaxWords,
            ImageUrl = prompt.ImageUrl,
            IsVipOnly = prompt.IsVipOnly,
            CreatedAt = prompt.CreatedAt
        };
    }

    public async Task<AdminPromptListItemDto> UpdatePromptAsync(Guid id, UpdatePromptRequestDto request)
    {
        var validationResult = await _updatePromptValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var prompt = await _dbContext.WritingPrompts
            .FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new PromptNotFoundException(id);

        prompt.WritingTypeId = request.WritingTypeId;
        prompt.Title = request.Title.Trim();
        prompt.Instructions = request.Instructions.Trim();
        prompt.Difficulty = request.Difficulty;
        prompt.TimeLimitMinutes = request.TimeLimitMinutes;
        prompt.MinWords = request.MinWords;
        prompt.MaxWords = request.MaxWords;
        prompt.ImageUrl = request.ImageUrl?.Trim();
        prompt.SampleAnswer = request.SampleAnswer?.Trim();
        prompt.IsVipOnly = request.IsVipOnly;

        await _dbContext.SaveChangesAsync();

        return new AdminPromptListItemDto
        {
            Id = prompt.Id,
            WritingTypeId = prompt.WritingTypeId,
            Title = prompt.Title,
            Difficulty = prompt.Difficulty,
            IsActive = prompt.IsActive,
            TimeLimitMinutes = prompt.TimeLimitMinutes,
            MinWords = prompt.MinWords,
            MaxWords = prompt.MaxWords,
            ImageUrl = prompt.ImageUrl,
            IsVipOnly = prompt.IsVipOnly,
            CreatedAt = prompt.CreatedAt
        };
    }

    public async Task DeletePromptAsync(Guid id)
    {
        var prompt = await _dbContext.WritingPrompts
            .FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new PromptNotFoundException(id);

        prompt.IsActive = false;
        await _dbContext.SaveChangesAsync();
    }
}