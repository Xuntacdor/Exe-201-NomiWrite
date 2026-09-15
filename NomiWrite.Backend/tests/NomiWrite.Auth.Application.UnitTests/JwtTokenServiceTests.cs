using System.IdentityModel.Tokens.Jwt;
using FluentAssertions;
using Microsoft.Extensions.Options;
using NomiWrite.Auth.Domain.Entities;
using NomiWrite.Auth.Domain.Enums;
using NomiWrite.Auth.Infrastructure.Options;
using NomiWrite.Auth.Infrastructure.Security;
using OptionsSet = Microsoft.Extensions.Options.Options;

namespace NomiWrite.Auth.Application.UnitTests;

public class JwtTokenServiceTests
{
    [Fact]
    public void GenerateAccessToken_EmitsStableGatewayClaims()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "student@example.com",
            FullName = "Student One",
            Role = UserRole.Student
        };
        var sut = new JwtTokenService(OptionsSet.Create(new JwtSettings
        {
            Secret = "phase-three-secret-with-at-least-32-bytes",
            Issuer = "NomiWrite.Auth",
            Audience = "NomiWrite.Services",
            ExpirationInMinutes = 30
        }));

        var (token, expiresAtUtc) = sut.GenerateAccessToken(user);

        token.Should().NotBeNullOrWhiteSpace();
        expiresAtUtc.Should().BeAfter(DateTime.UtcNow);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        jwt.Issuer.Should().Be("NomiWrite.Auth");
        jwt.Audiences.Should().ContainSingle().Which.Should().Be("NomiWrite.Services");
        jwt.Claims.Single(c => c.Type == JwtRegisteredClaimNames.Sub).Value.Should().Be(user.Id.ToString());
        jwt.Claims.Single(c => c.Type == JwtRegisteredClaimNames.Email).Value.Should().Be(user.Email);
        jwt.Claims.Single(c => c.Type == "role").Value.Should().Be(UserRole.Student.ToString());
        jwt.Claims.Single(c => c.Type == "full_name").Value.Should().Be(user.FullName);
    }
}
