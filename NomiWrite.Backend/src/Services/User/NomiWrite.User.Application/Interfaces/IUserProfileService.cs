using NomiWrite.User.Application.DTOs;

namespace NomiWrite.User.Application.Interfaces;

public interface IUserProfileService
{
    Task CreateProfileFromRegistrationAsync(Guid userId, string fullName);
    Task<UserProfileDto> GetProfileAsync(Guid userId);
    Task<UserProfileDto> UpdateProfileAsync(Guid userId, UpdateProfileRequestDto dto);
    Task<MyAccountDto> GetMyAccountAsync(Guid userId, string? accessToken);
}
