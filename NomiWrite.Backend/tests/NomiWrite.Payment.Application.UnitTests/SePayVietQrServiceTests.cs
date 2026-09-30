using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NomiWrite.Payment.Domain.Entities;
using NomiWrite.Payment.Domain.Enums;
using NomiWrite.Payment.Infrastructure.Options;
using NomiWrite.Payment.Infrastructure.Services;

namespace NomiWrite.Payment.Application.UnitTests;

public class SePayVietQrServiceTests
{
    private const string ApiKey = "sepay-secret-key";
    private const string BankId = "970436";
    private const string BankName = "VPBank";
    private const string AccountNumber = "1234567890";
    private const string AccountName = "NomiWrite Company";

    private static SePayVietQrService Build(Action<SePaySettings>? configure = null)
    {
        var settings = new SePaySettings
        {
            ApiKey = ApiKey,
            BankId = BankId,
            BankName = BankName,
            AccountNumber = AccountNumber,
            AccountName = AccountName
        };

        configure?.Invoke(settings);

        return new SePayVietQrService(
            Options.Create(settings),
            NullLogger<SePayVietQrService>.Instance);
    }

    private static PaymentOrder Order(string reference, decimal amount) => new()
    {
        OrderReference = reference,
        Amount = amount,
        Provider = PaymentProvider.VietQR,
        Status = PaymentStatus.Pending
    };

    // ── Authorization ──────────────────────────────────────────────────────────

    [Fact]
    public void IsAuthorized_ExactApikeyHeader_IsAccepted()
    {
        Build().IsAuthorized($"Apikey {ApiKey}").Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(ApiKey)]
    [InlineData("Bearer sepay-secret-key")]
    [InlineData("apikey sepay-secret-key")]
    [InlineData("Apikey")]
    [InlineData("Apikey ")]
    [InlineData("Apikey wrong-key")]
    [InlineData("Prefix Apikey sepay-secret-key")]
    public void IsAuthorized_AnythingElse_IsRejected(string? header)
    {
        Build().IsAuthorized(header).Should().BeFalse();
    }

    [Fact]
    public void IsAuthorized_SurroundingWhitespaceOnTheKey_IsTolerated()
    {
        // Header values are routinely whitespace-padded in transit, so the key itself
        // is trimmed. Only outer whitespace; the key must still match exactly.
        Build().IsAuthorized($"Apikey  {ApiKey}  ").Should().BeTrue();
    }

    [Fact]
    public void IsAuthorized_HeaderIsNotTrimmedByCaller_StillRejects()
    {
        Build().IsAuthorized($"Apikey {ApiKey} extra").Should().BeFalse();
    }

    [Fact]
    public void IsAuthorized_MissingApiKey_FailsClosed()
    {
        var sut = Build(s => s.ApiKey = "  ");

        sut.IsConfigured.Should().BeFalse();
        sut.IsAuthorized("Apikey anything").Should().BeFalse();
        sut.IsAuthorized(null).Should().BeFalse();
    }

    [Fact]
    public void IsConfigured_TracksApiKeyPresence()
    {
        Build().IsConfigured.Should().BeTrue();
        Build(s => s.ApiKey = string.Empty).IsConfigured.Should().BeFalse();
    }

    // ── Receiving account ──────────────────────────────────────────────────────

    [Fact]
    public void ExpectedAccountNumber_IsTrimmedOrNull()
    {
        Build().ExpectedAccountNumber.Should().Be(AccountNumber);
        Build(s => s.AccountNumber = " 9876543210 ").ExpectedAccountNumber.Should().Be("9876543210");
        Build(s => s.AccountNumber = "   ").ExpectedAccountNumber.Should().BeNull();
    }

    // ── VietQR checkout payload ────────────────────────────────────────────────

    [Fact]
    public void BuildCheckout_ProducesVietQrImageUrlWithAmountAndReference()
    {
        var reference = "NWQ0123456789ABCDEFGHI";

        var checkout = Build().BuildCheckout(Order(reference, 199_000m));

        checkout.BankId.Should().Be(BankId);
        checkout.BankName.Should().Be(BankName);
        checkout.AccountNumber.Should().Be(AccountNumber);
        checkout.AccountName.Should().Be(AccountName);
        checkout.Amount.Should().Be(199_000m);
        checkout.TransferContent.Should().Be(reference);
        checkout.QrImageUrl.Should().Be(
            $"https://img.vietqr.io/image/{BankId}-{AccountNumber}-compact2.png" +
            $"?amount=199000&addInfo={reference}&accountName=NomiWrite%20Company");
    }

    [Fact]
    public void BuildCheckout_RoundsFractionalDongBecauseBanksRejectIt()
    {
        // A 20% promo on 199000 leaves 159200.00 in storage.
        Build().BuildCheckout(Order("NWQ0123456789ABCDEFGHI", 159_200.00m))
            .QrImageUrl.Should().Contain("amount=159200");

        Build().BuildCheckout(Order("NWQ0123456789ABCDEFGHI", 199_000.50m))
            .QrImageUrl.Should().Contain("amount=199001");
    }

    [Fact]
    public void BuildCheckout_EscapesAccountNameWithSpaces()
    {
        var checkout = Build(s => s.AccountName = "Công Ty TNHH ABC & XYZ")
            .BuildCheckout(Order("NWQ0123456789ABCDEFGHI", 100_000m));

        checkout.QrImageUrl.Should().Contain("accountName=C%C3%B4ng%20Ty%20TNHH%20ABC%20%26%20XYZ");
    }

    [Fact]
    public void BuildCheckout_TrimsSurroundedWhitespaceBeforeValidating()
    {
        Build(s => s.BankId = "  970422  ")
            .BuildCheckout(Order("NWQ0123456789ABCDEFGHI", 100_000m))
            .BankId.Should().Be("970422");
    }

    [Fact]
    public void BuildCheckout_HonoursACustomImageHost()
    {
        var checkout = Build(s => s.QrImageBaseUrl = "https://cdn.example.com/qr/")
            .BuildCheckout(Order("NWQ0123456789ABCDEFGHI", 100_000m));

        checkout.QrImageUrl.Should().StartWith($"https://cdn.example.com/qr/{BankId}-{AccountNumber}-compact2.png");
    }

    [Fact]
    public void BuildCheckout_BankNameIsOptionalAndDoesNotFailTheCheckout()
    {
        // The QR only needs the numeric bank code, so an unconfigured bank name
        // degrades the label instead of blocking the payment.
        var checkout = Build(s => s.BankName = "   ").BuildCheckout(Order("NWQ0123456789ABCDEFGHI", 100_000m));

        checkout.BankName.Should().BeEmpty();
        checkout.QrImageUrl.Should().Contain($"{BankId}-{AccountNumber}-compact2.png");
    }

    [Fact]
    public void BuildCheckout_BankNameIsTrimmed()
    {
        Build(s => s.BankName = "  VPBank  ").BuildCheckout(Order("NWQ0123456789ABCDEFGHI", 100_000m))
            .BankName.Should().Be("VPBank");
    }

    [Theory]
    [InlineData("MBBank")]
    [InlineData("MB")]
    [InlineData("vpbank")]
    [InlineData("97042a")]
    public void BuildCheckout_BankIdMustBeTheNumericCode_Throws(string badBankId)
    {
        // A bank *name* here produces a well-formed URL that 404s, so the customer
        // sees an unscanable QR instead of an error.
        var ex = FluentActions.Invoking(() => Build(s => s.BankId = badBankId)
                .BuildCheckout(Order("NWQ0123456789ABCDEFGHI", 100_000m)));

        ex.Should().Throw<InvalidOperationException>().WithMessage("*numeric VietQR bank code*");
    }

    [Theory]
    [InlineData("0200.269-329999")]
    [InlineData("account")]
    public void BuildCheckout_AccountNumberMustBeDigits_Throws(string badAccountNumber)
    {
        var ex = FluentActions.Invoking(() => Build(s => s.AccountNumber = badAccountNumber)
                .BuildCheckout(Order("NWQ0123456789ABCDEFGHI", 100_000m)));

        ex.Should().Throw<InvalidOperationException>().WithMessage("*must be digits only*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void BuildCheckout_MissingBankConfiguration_Throws(string blank)
    {
        var build = (Action<SePaySettings> configure) => Build(configure).BuildCheckout(Order("NWQ0123456789ABCDEFGHI", 100_000m));

        FluentActions.Invoking(() => build(s => s.BankId = blank)).Should().Throw<InvalidOperationException>();
        FluentActions.Invoking(() => build(s => s.AccountNumber = blank)).Should().Throw<InvalidOperationException>();
        FluentActions.Invoking(() => build(s => s.AccountName = blank)).Should().Throw<InvalidOperationException>();
    }
}
