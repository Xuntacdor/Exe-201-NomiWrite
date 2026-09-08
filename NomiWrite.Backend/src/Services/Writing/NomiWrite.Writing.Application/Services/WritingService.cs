using FluentValidation;
using FluentValidation.Results;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using NomiWrite.Shared.Contracts.Events.Writing;
using NomiWrite.Writing.Application.DTOs;
using NomiWrite.Writing.Application.Exceptions;
using NomiWrite.Writing.Application.Interfaces;
using NomiWrite.Writing.Domain.Entities;
using NomiWrite.Writing.Domain.Enums;

namespace NomiWrite.Writing.Application.Services;

public class WritingService : IWritingService
{
    private readonly IWritingDbContext _dbContext;
    private readonly IValidator<CreateSubmissionRequestDto> _createSubmissionValidator;
    private readonly IValidator<UpdateSubmissionRequestDto> _updateSubmissionValidator;
    private readonly IPublishEndpoint _publishEndpoint;

    public WritingService(
        IWritingDbContext dbContext,
        IValidator<CreateSubmissionRequestDto> createSubmissionValidator,
        IValidator<UpdateSubmissionRequestDto> updateSubmissionValidator,
        IPublishEndpoint publishEndpoint)
    {
        _dbContext = dbContext;
        _createSubmissionValidator = createSubmissionValidator;
        _updateSubmissionValidator = updateSubmissionValidator;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<IReadOnlyList<WritingTypeDto>> GetWritingTypesAsync()
    {
        return await _dbContext.WritingTypes
            .AsNoTracking()
            .Where(t => t.IsActive)
            .OrderBy(t => t.Name)
            .Select(t => new WritingTypeDto
            {
                Id = t.Id,
                Name = t.Name,
                Category = t.Category,
                Description = t.Description
            })
            .ToListAsync();
    }

    public async Task<IReadOnlyList<WritingPromptListItemDto>> GetPromptsAsync(
        Guid? typeId,
        DifficultyLevel? difficulty,
        bool random)
    {
        var query = _dbContext.WritingPrompts
            .AsNoTracking()
            .Where(p => p.IsActive);

        if (typeId.HasValue)
            query = query.Where(p => p.WritingTypeId == typeId.Value);

        if (difficulty.HasValue)
            query = query.Where(p => p.Difficulty == difficulty.Value);

        if (random)
        {
            var prompt = await query
                .OrderBy(_ => EF.Functions.Random())
                .Select(p => new WritingPromptListItemDto
                {
                    Id = p.Id,
                    WritingTypeId = p.WritingTypeId,
                    WritingTypeName = p.WritingType!.Name,
                    Title = p.Title,
                    Difficulty = p.Difficulty
                })
                .FirstOrDefaultAsync();

            return prompt is null
                ? Array.Empty<WritingPromptListItemDto>()
                : new[] { prompt };
        }

        return await query
            .OrderBy(p => p.Title)
            .Select(p => new WritingPromptListItemDto
            {
                Id = p.Id,
                WritingTypeId = p.WritingTypeId,
                WritingTypeName = p.WritingType!.Name,
                Title = p.Title,
                Difficulty = p.Difficulty
            })
            .ToListAsync();
    }

    public async Task<WritingPromptDto> GetPromptByIdAsync(Guid id)
    {
        var prompt = await _dbContext.WritingPrompts
            .AsNoTracking()
            .Where(p => p.IsActive && p.Id == id)
            .Select(p => new WritingPromptDto
            {
                Id = p.Id,
                WritingTypeId = p.WritingTypeId,
                WritingTypeName = p.WritingType!.Name,
                Title = p.Title,
                Instructions = p.Instructions,
                Difficulty = p.Difficulty
            })
            .FirstOrDefaultAsync();

        if (prompt is null)
            throw new PromptNotFoundException(id);

        return prompt;
    }

    public async Task<SubmissionResponseDto> CreateSubmissionAsync(Guid userId, CreateSubmissionRequestDto dto)
    {
        var validationResult = await _createSubmissionValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var prompt = await _dbContext.WritingPrompts
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == dto.WritingPromptId && p.IsActive);

        if (prompt is null)
            throw new PromptNotFoundException(dto.WritingPromptId);

        var now = DateTime.UtcNow;
        var submission = new WritingSubmission
        {
            UserId = userId,
            WritingPromptId = dto.WritingPromptId,
            Content = string.Empty,
            WordCount = 0,
            IsTimed = dto.IsTimed,
            Status = SubmissionStatus.Draft,
            StartedAt = now,
            CreatedAt = now,
            UpdatedAt = now
        };

        _dbContext.WritingSubmissions.Add(submission);
        await _dbContext.SaveChangesAsync();

        return ToSubmissionResponse(submission, prompt.Title);
    }

    public async Task<SubmissionResponseDto> UpdateSubmissionAsync(
        Guid userId,
        Guid submissionId,
        UpdateSubmissionRequestDto dto)
    {
        var validationResult = await _updateSubmissionValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var submission = await GetOwnedSubmissionAsync(userId, submissionId);
        if (submission.Status != SubmissionStatus.Draft)
            throw new SubmissionNotEditableException(submissionId);

        submission.Content = dto.Content;
        submission.WordCount = CountWords(dto.Content);

        await _dbContext.SaveChangesAsync();

        return await ToSubmissionResponseAsync(submission);
    }

    public async Task<SubmissionResponseDto> SubmitSubmissionAsync(Guid userId, Guid submissionId)
    {
        var submission = await GetOwnedSubmissionAsync(userId, submissionId);
        if (submission.Status != SubmissionStatus.Draft)
            throw new SubmissionNotEditableException(submissionId);

        if (string.IsNullOrWhiteSpace(submission.Content))
        {
            throw new ValidationException(
                new[] { new ValidationFailure("Content", "Content cannot be empty when submitting.") });
        }

        var now = DateTime.UtcNow;
        submission.Status = SubmissionStatus.Submitted;
        submission.SubmittedAt = now;

        await _dbContext.SaveChangesAsync();

        await _publishEndpoint.Publish(new WritingSubmittedEvent(
            submission.Id,
            submission.UserId,
            submission.WritingPromptId,
            submission.Content,
            submission.WordCount,
            submission.SubmittedAt.Value));

        return await ToSubmissionResponseAsync(submission);
    }

    public async Task<SubmissionResponseDto> GetSubmissionByIdAsync(Guid userId, Guid submissionId)
    {
        var submission = await GetOwnedSubmissionAsync(userId, submissionId);
        return await ToSubmissionResponseAsync(submission);
    }

    public async Task<IReadOnlyList<SubmissionListItemDto>> GetUserSubmissionsAsync(Guid userId)
    {
        return await _dbContext.WritingSubmissions
            .AsNoTracking()
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => new SubmissionListItemDto
            {
                Id = s.Id,
                WritingPromptId = s.WritingPromptId,
                PromptTitle = s.WritingPrompt!.Title,
                WordCount = s.WordCount,
                Status = s.Status,
                StartedAt = s.StartedAt,
                SubmittedAt = s.SubmittedAt
            })
            .ToListAsync();
    }

    private async Task<WritingSubmission> GetOwnedSubmissionAsync(Guid userId, Guid submissionId)
    {
        var submission = await _dbContext.WritingSubmissions
            .FirstOrDefaultAsync(s => s.Id == submissionId);

        if (submission is null)
            throw new SubmissionNotFoundException(submissionId);

        if (submission.UserId != userId)
            throw new ForbiddenSubmissionAccessException(submissionId);

        return submission;
    }

    private async Task<SubmissionResponseDto> ToSubmissionResponseAsync(WritingSubmission submission)
    {
        var promptTitle = await _dbContext.WritingPrompts
            .AsNoTracking()
            .Where(p => p.Id == submission.WritingPromptId)
            .Select(p => p.Title)
            .FirstOrDefaultAsync() ?? string.Empty;

        return ToSubmissionResponse(submission, promptTitle);
    }

    private static SubmissionResponseDto ToSubmissionResponse(WritingSubmission submission, string promptTitle)
    {
        return new SubmissionResponseDto
        {
            Id = submission.Id,
            WritingPromptId = submission.WritingPromptId,
            PromptTitle = promptTitle,
            Content = submission.Content,
            WordCount = submission.WordCount,
            Status = submission.Status,
            StartedAt = submission.StartedAt,
            SubmittedAt = submission.SubmittedAt
        };
    }

    private static int CountWords(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return 0;

        return content
            .Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .Length;
    }
}