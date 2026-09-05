using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NomiWrite.Auth.Application.Interfaces;
using NomiWrite.Auth.Domain.Entities;
using NomiWrite.Auth.Infrastructure.Options;

namespace NomiWrite.Auth.Infrastructure.Security;

public class JwtTokenService : IJwtTokenService
{
    private const int MinimumSecretByteLength = 32;

    private readonly JwtSettings _settings;
    private readonly SigningCredentials _signingCredentials;

    public JwtTokenService(IOptions<JwtSettings> options)
    {
        _settings = options.Value;

        var keyBytes = Encoding.UTF8.GetBytes(_settings.Secret);
        if (keyBytes.Length < MinimumSecretByteLength)
            throw new InvalidOperationException($"JwtSettings:Secret must be at least {MinimumSecretByteLength} bytes long.");

        _signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(keyBytes),
            SecurityAlgorithms.HmacSha256);
    }

    public (string Token, DateTime ExpiresAtUtc) GenerateAccessToken(User user)
    {
        var expiresAtUtc = DateTime.UtcNow.AddMinutes(_settings.ExpirationInMinutes);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("role", user.Role.ToString()),
            new Claim("full_name", user.FullName)
        };

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expiresAtUtc,
            signingCredentials: _signingCredentials);

        var serializedToken = new JwtSecurityTokenHandler().WriteToken(token);
        return (serializedToken, expiresAtUtc);
    }

    public string GenerateRefreshToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(randomBytes);
    }
}