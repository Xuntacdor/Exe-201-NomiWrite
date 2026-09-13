using FluentAssertions;
using NomiWrite.Auth.Infrastructure.Security;

namespace NomiWrite.Auth.Application.UnitTests;

public class PasswordHasherTests
{
    [Fact]
    public void HashPassword_RoundTrips_Success()
    {
        var sut = new PasswordHasher();

        var hash = sut.HashPassword("S3cure#Pass42");

        hash.Should().NotBeNullOrWhiteSpace();
        sut.VerifyPassword("S3cure#Pass42", hash).Should().BeTrue();
    }

    [Fact]
    public void HashPassword_WrongPassword_Fails()
    {
        var sut = new PasswordHasher();
        var hash = sut.HashPassword("S3cure#Pass42");

        sut.VerifyPassword("wrong-password", hash).Should().BeFalse();
    }

    [Fact]
    public void HashPassword_SameInput_DifferentSalt_ProducesDifferentHash()
    {
        var sut = new PasswordHasher();

        var hash1 = sut.HashPassword("diff-me");
        var hash2 = sut.HashPassword("diff-me");

        hash1.Should().NotBe(hash2);
        sut.VerifyPassword("diff-me", hash1).Should().BeTrue();
        sut.VerifyPassword("diff-me", hash2).Should().BeTrue();
    }
}