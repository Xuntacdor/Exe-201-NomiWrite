using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NomiWrite.User.Application.DTOs;
using NomiWrite.User.Application.Exceptions;
using NomiWrite.User.Application.Interfaces;
using NomiWrite.User.Domain.Entities;

namespace NomiWrite.User.Application.Services;

public class UserProfileService : IUserProfileService
{
    private readonly IUserDbContext _dbContext;
    private readonly IValidator<UpdateProfileRequestDto> _updateProfileValidator;
    private readonly ISubscriptionStatusClient _subscriptionStatusClient;
    private readonly ILogger<UserProfileService> _logger;

    public UserProfileService(
        IUserDbContext dbContext,
        IValidator<UpdateProfileRequestDto> updateProfileValidator,
        ISubscriptionStatusClient subscriptionStatusClient,
        ILogger<UserProfileService> logger)
    {
        _dbContext = dbContext;
        _updateProfileValidator = updateProfileValidator;
        _subscriptionStatusClient = subscriptionStatusClient;
        _logger = logger;
    }

    public async Task CreateProfileFromRegistrationAsync(Guid userId, string fullName)
    {
        var existing = await _dbContext.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (existing is not null)
            return;

        var now = DateTime.UtcNow;
        _dbContext.UserProfiles.Add(new UserProfile
        {
            UserId = userId,
            DisplayName = fullName,
            CreatedAt = now,
            UpdatedAt = now
        });

        await _dbContext.SaveChangesAsync();
    }

    public async Task<UserProfileDto> GetProfileAsync(Guid userId)
    {
        var profile = await _dbContext.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId)
            ?? throw new ProfileNotFoundException(userId);

        return ToProfileDto(profile);
    }

    public async Task<UserProfileDto> UpdateProfileAsync(Guid userId, UpdateProfileRequestDto dto)
    {
        var validationResult = await _updateProfileValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var profile = await _dbContext.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId)
            ?? throw new ProfileNotFoundException(userId);

        if (dto.DisplayName is not null)
            profile.DisplayName = dto.DisplayName;

        if (dto.AvatarUrl is not null)
            profile.AvatarUrl = dto.AvatarUrl;

        if (dto.Bio is not null)
            profile.Bio = dto.Bio;

        if (dto.TargetExam is not null)
            profile.TargetExam = dto.TargetExam;

        if (dto.TargetBand is not null)
            profile.TargetBand = dto.TargetBand;

        if (dto.EnglishLevel is not null)
            profile.EnglishLevel = dto.EnglishLevel;

        profile.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return ToProfileDto(profile);
    }

    public async Task<MyAccountDto> GetMyAccountAsync(Guid userId, string? accessToken)
    {
        var profile = await _dbContext.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId)
            ?? throw new ProfileNotFoundException(userId);

        bool hasActiveSubscription = false;
        string? subscriptionPlanName = null;
        DateTime? subscriptionEndDate = null;

        try
        {
            var subscription = await _subscriptionStatusClient.GetCurrentSubscriptionAsync(userId, accessToken);
            hasActiveSubscription = subscription.HasActiveSubscription;
            subscriptionPlanName = subscription.PlanName;
            subscriptionEndDate = subscription.EndDate;
        }
        catch (Exception exception)
        {
            _logger.LogWarning(
                exception,
                "Failed to retrieve subscription status for user {UserId}; defaulting to no active subscription.",
                userId);
        }

        return new MyAccountDto
        {
            UserId = profile.UserId,
            DisplayName = profile.DisplayName,
            AvatarUrl = profile.AvatarUrl,
            Bio = profile.Bio,
            TargetExam = profile.TargetExam,
            TargetBand = profile.TargetBand,
            EnglishLevel = profile.EnglishLevel,
            HasActiveSubscription = hasActiveSubscription,
            SubscriptionPlanName = subscriptionPlanName,
            SubscriptionEndDate = subscriptionEndDate
        };
    }

    private static UserProfileDto ToProfileDto(UserProfile profile)
    {
        return new UserProfileDto
        {
            UserId = profile.UserId,
            DisplayName = profile.DisplayName,
            AvatarUrl = profile.AvatarUrl,
            Bio = profile.Bio,
            TargetExam = profile.TargetExam,
            TargetBand = profile.TargetBand,
            EnglishLevel = profile.EnglishLevel
        };
    }
}