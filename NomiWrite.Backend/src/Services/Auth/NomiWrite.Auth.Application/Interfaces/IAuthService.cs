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
}
