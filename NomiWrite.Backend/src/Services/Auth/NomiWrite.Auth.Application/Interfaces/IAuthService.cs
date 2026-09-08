using NomiWrite.Auth.Application.DTOs;

namespace NomiWrite.Auth.Application.Interfaces;

/// <summary>
/// Core authentication service — registration, login, token management.
/// </summary>
public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request);
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request);
    Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request);
    Task LogoutAsync(Guid userId);
    Task<AuthResultDto> VerifyEmailAsync(VerifyEmailRequestDto request);
    Task<AuthResultDto> ResendVerificationEmailAsync(ResendVerificationEmailRequestDto request, Guid userId);
    Task<AuthResultDto> ForgotPasswordAsync(ForgotPasswordRequestDto request);
    Task<AuthResultDto> ResetPasswordAsync(ResetPasswordRequestDto request);
    Task<AuthResultDto> DeactivateAccountAsync(Guid userId);
}
