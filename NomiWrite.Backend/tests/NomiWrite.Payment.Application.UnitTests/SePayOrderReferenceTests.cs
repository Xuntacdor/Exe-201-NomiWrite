using FluentAssertions;
using NomiWrite.Payment.Application.DTOs;
using NomiWrite.Payment.Application.Services;

namespace NomiWrite.Payment.Application.UnitTests;

public class SePayOrderReferenceTests
{
    [Fact]
    public void Create_ProducesAReferenceVietQrAndSePayCanBothCarry()
    {
        var reference = SePayOrderReference.Create();

        // VietQR truncates addInfo at 25 characters, and SePay's code extractor
        // only matches [0-9A-Z]. Both hold only for a short alphanumeric reference.
        reference.Should().HaveLength(23);
        reference.Length.Should().BeLessThanOrEqualTo(25);
        reference.Should().StartWith("NWQ");
        reference.Should().MatchRegex("^NWQ[0-9A-Z]{20}$");
    }

    [Fact]
    public void Create_IsAlwaysTheSameLength()
    {
        // The reference doubles as the VietQR addInfo value, so a variable length
        // would make some orders unmatchable.
        for (var i = 0; i < 500; i++)
        {
            SePayOrderReference.Create().Should().HaveLength(SePayOrderReference.TotalLength);
        }
    }

    [Fact]
    public void Create_DoesNotRepeat()
    {
        var references = Enumerable.Range(0, 5_000)
            .Select(_ => SePayOrderReference.Create())
            .ToList();

        references.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void ExtractCandidates_PrefersSePayCode()
    {
        var reference = SePayOrderReference.Create();

        var candidates = SePayOrderReference.ExtractCandidates(new SePayWebhookDto
        {
            Code = reference,
            Content = $"noise {reference} noise"
        });

        candidates.Should().ContainSingle().Which.Should().Be(reference);
    }

    [Fact]
    public void ExtractCandidates_ScansContentWhenCodeIsNull()
    {
        var reference = SePayOrderReference.Create();

        var candidates = SePayOrderReference.ExtractCandidates(new SePayWebhookDto
        {
            Code = null,
            Content = $"thanh toan don hang {reference} NomiWrite"
        });

        candidates.Should().ContainSingle().Which.Should().Be(reference);
    }

    [Fact]
    public void ExtractCandidates_UppercasesAndDeduplicates()
    {
        var reference = SePayOrderReference.Create();
        var lower = reference.ToLowerInvariant();

        var candidates = SePayOrderReference.ExtractCandidates(new SePayWebhookDto
        {
            Code = lower,
            Content = lower,
            Description = reference
        });

        candidates.Should().ContainSingle().Which.Should().Be(reference);
    }

    [Fact]
    public void ExtractCandidates_AlsoFindsLegacyPayReferences()
    {
        var legacy = $"PAY-{Guid.NewGuid():N}".ToUpperInvariant();

        var candidates = SePayOrderReference.ExtractCandidates(new SePayWebhookDto
        {
            Content = $"Thanh toan don hang {legacy}"
        });

        candidates.Should().ContainSingle().Which.Should().Be(legacy);
    }

    [Fact]
    public void ExtractCandidates_ReturnsNothingForAnUnrelatedTransfer()
    {
        var candidates = SePayOrderReference.ExtractCandidates(new SePayWebhookDto
        {
            Code = null,
            Content = "CHUYEN TIEN SINH HOAT",
            Description = "NGUYEN VAN A"
        });

        candidates.Should().BeEmpty();
    }

    [Fact]
    public void ExtractCandidates_DoesNotMatchTruncatedOrOverlongTokens()
    {
        var candidates = SePayOrderReference.ExtractCandidates(new SePayWebhookDto
        {
            Content = "NWQ123 chuyen tien NWQABCDEFGHIJKLMNOPQRSTUVWX chuyen tien"
        });

        // Too short and too long: neither is a reference this system ever issued.
        candidates.Should().BeEmpty();
    }

    [Fact]
    public void ExtractCandidates_KeepsSeveralDistinctCandidates()
    {
        var first = SePayOrderReference.Create();
        var second = SePayOrderReference.Create();

        var candidates = SePayOrderReference.ExtractCandidates(new SePayWebhookDto
        {
            Content = $"{first} {second}"
        });

        candidates.Should().HaveCount(2);
    }
}
