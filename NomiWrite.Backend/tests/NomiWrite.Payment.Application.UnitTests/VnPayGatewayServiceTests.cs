using System.Security.Cryptography;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NomiWrite.Payment.Domain.Entities;
using NomiWrite.Payment.Infrastructure.Options;
using NomiWrite.Payment.Infrastructure.Services;

namespace NomiWrite.Payment.Application.UnitTests;

public class VnPayGatewayServiceTests
{
    private const string HashSecret = "test-hash-secret";
    private const string TmnCode = "VNP12345";

    private static VnPayGatewayService Build() => new(
        Options.Create(new VnPaySettings
        {
            TmnCode = TmnCode,
            HashSecret = HashSecret,
            BaseUrl = "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html",
            ReturnUrl = "http://localhost:3000/payment/vnpay/return"
        }),
        NullLogger<VnPayGatewayService>.Instance);

    private static string HmacSha512(string key, string input)
    {
        var builder = new StringBuilder();
        using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(key));
        foreach (var @byte in hmac.ComputeHash(Encoding.UTF8.GetBytes(input)))
            builder.Append(@byte.ToString("x2"));
        return builder.ToString();
    }

    private static string BuildHashInput(IEnumerable<KeyValuePair<string, string>> pairs)
    {
        var builder = new StringBuilder();
        foreach (var pair in pairs)
        {
            builder.Append(Uri.EscapeDataString(pair.Key));
            builder.Append('=');
            builder.Append(Uri.EscapeDataString(pair.Value));
            builder.Append('&');
        }
        if (builder.Length > 0) builder.Length -= 1;
        return builder.ToString();
    }

    #region U-P3 — VNPay signature

    [Fact]
    public void VerifyIpnAsync_ValidSignature_ReturnsParsedResult()
    {
        var sut = Build();

        var parameters = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            ["vnp_TmnCode"] = TmnCode,
            ["vnp_Amount"] = "1234500",
            ["vnp_TxnRef"] = "PAY-123456789",
            ["vnp_TransactionNo"] = "987654",
            ["vnp_ResponseCode"] = "00",
            ["vnp_TransactionStatus"] = "00"
        };
        parameters["vnp_SecureHash"] = HmacSha512(HashSecret, BuildHashInput(parameters));
        parameters["vnp_SecureHashType"] = "SHA512";

        var result = sut.VerifyIpnAsync(parameters);

        result.IsSignatureValid.Should().BeTrue();
        result.OrderReference.Should().Be("PAY-123456789");
        result.ProviderTransactionId.Should().Be("987654");
        result.ResponseCode.Should().Be("00");
        result.TransactionStatus.Should().Be("00");
        result.Amount.Should().Be(12_345m);
    }

    [Fact]
    public void VerifyIpnAsync_TamperedAmount_InvalidSignature()
    {
        var sut = Build();

        var parameters = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            ["vnp_TmnCode"] = TmnCode,
            ["vnp_Amount"] = "1234500",
            ["vnp_TxnRef"] = "PAY-123456789",
            ["vnp_TransactionNo"] = "987654",
            ["vnp_ResponseCode"] = "00",
            ["vnp_TransactionStatus"] = "00"
        };
        parameters["vnp_SecureHash"] = HmacSha512(HashSecret, BuildHashInput(parameters));

        // Attacker modifies the amount AFTER the signature was issued.
        parameters["vnp_Amount"] = "99999999";

        var result = sut.VerifyIpnAsync(parameters);

        result.IsSignatureValid.Should().BeFalse();
    }

    [Fact]
    public void VerifyIpnAsync_MissingSecureHash_ReturnsInvalid()
    {
        var sut = Build();
        var parameters = new Dictionary<string, string>
        {
            ["vnp_Amount"] = "1234500",
            ["vnp_TxnRef"] = "PAY-123456789"
        };

        var result = sut.VerifyIpnAsync(parameters);

        result.IsSignatureValid.Should().BeFalse();
        result.OrderReference.Should().BeEmpty();
    }

    [Theory]
    [InlineData("00", "00", true)]
    [InlineData("24", "00", false)]
    [InlineData("00", "02", false)]
    [InlineData("99", "01", false)]
    public void IsPaymentSuccessful_RequiresCode00AndStatus00(string responseCode, string transactionStatus, bool expected)
    {
        var result = new VnPayIpnResult(
            IsSignatureValid: true,
            OrderReference: "PAY-1",
            ProviderTransactionId: "tx",
            ResponseCode: responseCode,
            TransactionStatus: transactionStatus,
            Amount: 100m);

        VnPayGatewayService.IsPaymentSuccessful(result).Should().Be(expected);
    }

    [Fact]
    public async Task CreatePaymentAsync_BuildsUrlWithSecureHash()
    {
        var sut = Build();
        var order = new PaymentOrder
        {
            Id = Guid.NewGuid(),
            OrderReference = "PAY-REF-1",
            Amount = 100_000m,
            CreatedAt = new DateTime(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc)
        };

        var url = await sut.CreatePaymentAsync(order, "127.0.0.1");

        url.Should().StartWith("https://sandbox.vnpayment.vn/");
        url.Should().Contain("vnp_Amount=10000000");
        url.Should().Contain("vnp_TxnRef=PAY-REF-1");
        url.Should().Contain("vnp_SecureHash=");
    }

    #endregion
}