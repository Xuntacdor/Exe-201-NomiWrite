using NomiWrite.Auth.Domain.Entities;

namespace NomiWrite.Auth.Application.Interfaces;

/// <summary>
/// JWT token generation and validation.
/// </summary>
public interface ITokenService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
}
