using System.Security.Cryptography;
using FluentAssertions;
using Microsoft.Extensions.Options;
using NomiWrite.AICoordinator.Infrastructure.Options;
using NomiWrite.AICoordinator.Infrastructure.Security;

namespace NomiWrite.AICoordinator.Application.UnitTests;

public class AesAiSecretProtectorTests
{
    [Fact]
    public void ProtectAndUnprotect_RoundTripsWithoutEmbeddingPlaintext()
    {
        var sut = Build(RandomNumberGenerator.GetBytes(32));
        const string secret = "realistic-api-key-value-for-testing";

        var protectedValue = sut.Protect(secret);

        protectedValue.Should().NotContain(secret);
        protectedValue.Should().StartWith("v1.");
        sut.Unprotect(protectedValue).Should().Be(secret);
    }

    [Fact]
    public void Unprotect_WithDifferentMasterKey_FailsAuthentication()
    {
        var ciphertext = Build(RandomNumberGenerator.GetBytes(32)).Protect("secret-value");
        var otherProtector = Build(RandomNumberGenerator.GetBytes(32));

        var action = () => otherProtector.Unprotect(ciphertext);

        action.Should().Throw<CryptographicException>();
    }

    private static AesAiSecretProtector Build(byte[] key) =>
        new(Options.Create(new AiSecretProtectionSettings
        {
            MasterKey = Convert.ToBase64String(key)
        }));
}
