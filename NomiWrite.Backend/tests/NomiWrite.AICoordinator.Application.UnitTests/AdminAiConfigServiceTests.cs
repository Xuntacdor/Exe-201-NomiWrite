using FluentAssertions;
using NSubstitute;
using NomiWrite.AICoordinator.Application.DTOs;
using NomiWrite.AICoordinator.Application.Interfaces;
using NomiWrite.AICoordinator.Application.Services;
using NomiWrite.AICoordinator.Application.UnitTests.Persistence;
using NomiWrite.AICoordinator.Application.Validation;
using NomiWrite.AICoordinator.Domain.Entities;

namespace NomiWrite.AICoordinator.Application.UnitTests;

public class AdminAiConfigServiceTests
{
    [Fact]
    public async Task UpdateActiveConfigAsync_NewApiKey_StoresOnlyProtectedValue()
    {
        await using var context = TestGradingDbContext.Create();
        var provider = Substitute.For<IAiGradingProvider>();
        var sut = Build(context, provider);

        var result = await sut.UpdateActiveConfigAsync(new UpdateAiGradingConfigRequestDto
        {
            ProviderName = "Gemini",
            ModelName = "gemini-2.5-flash",
            ApiKey = "plain-api-key-that-must-not-be-stored"
        });

        var stored = context.AiGradingConfigs.Single(c => c.IsActive);
        stored.ApiKeyCiphertext.Should().Be("protected:plain-api-key-that-must-not-be-stored");
        stored.ApiKeyCiphertext.Should().NotBe("plain-api-key-that-must-not-be-stored");
        result.HasStoredApiKey.Should().BeTrue();
        result.ApiKeyHint.Should().Be("••••ored");
        provider.Received(1).InvalidateActiveConfigCache();
    }

    [Fact]
    public async Task UpdateActiveConfigAsync_BlankApiKey_PreservesExistingCiphertext()
    {
        await using var context = TestGradingDbContext.Create();
        context.AiGradingConfigs.Add(new AiGradingConfig
        {
            ProviderName = "Gemini",
            ModelName = "old-model",
            ApiKeyCiphertext = "protected:existing-secret",
            IsActive = true
        });
        await context.SaveChangesAsync();
        var sut = Build(context, Substitute.For<IAiGradingProvider>());

        await sut.UpdateActiveConfigAsync(new UpdateAiGradingConfigRequestDto
        {
            ProviderName = "Gemini",
            ModelName = "new-model"
        });

        context.AiGradingConfigs.Single(c => c.IsActive).ApiKeyCiphertext.Should().Be("protected:existing-secret");
    }

    [Fact]
    public async Task UpdateActiveConfigAsync_ClearApiKey_RemovesStoredKey()
    {
        await using var context = TestGradingDbContext.Create();
        context.AiGradingConfigs.Add(new AiGradingConfig
        {
            ProviderName = "Gemini",
            ModelName = "old-model",
            ApiKeyCiphertext = "protected:existing-secret",
            IsActive = true
        });
        await context.SaveChangesAsync();
        var sut = Build(context, Substitute.For<IAiGradingProvider>());

        var result = await sut.UpdateActiveConfigAsync(new UpdateAiGradingConfigRequestDto
        {
            ProviderName = "Gemini",
            ModelName = "new-model",
            ClearApiKey = true
        });

        context.AiGradingConfigs.Single(c => c.IsActive).ApiKeyCiphertext.Should().BeNull();
        result.HasStoredApiKey.Should().BeFalse();
        result.ApiKeyHint.Should().BeNull();
    }

    private static AdminAiConfigService Build(TestGradingDbContext context, IAiGradingProvider provider) =>
        new(context, provider, new TestSecretProtector(), new UpdateAiGradingConfigRequestValidator());

    private sealed class TestSecretProtector : IAiSecretProtector
    {
        public bool IsConfigured => true;
        public string Protect(string plaintext) => $"protected:{plaintext}";
        public string Unprotect(string ciphertext) => ciphertext[10..];
    }
}
