using System.Security.Cryptography;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NomiWrite.Payment.Infrastructure.Options;
using NomiWrite.Payment.Infrastructure.Services;

namespace NomiWrite.Payment.Application.UnitTests;

public class MomoGatewayServiceTests
{
    private const string AccessKey = "access-key-test";
    private const string SecretKey = "secret-key-test";
    private const string PartnerCode = "MOMO123";

    private static MomoGatewayService Build() => new(
        Options.Create(new MomoSettings
        {
            PartnerCode = PartnerCode,
            AccessKey = AccessKey,
            SecretKey = SecretKey,
            Endpoint = "https://test-payment.momo.vn/v2/gateway/api/create",
            RedirectUrl = "http://localhost:3000/payment/momo/return",
            IpnUrl = "http://localhost:3000/payment/momo/ipn"
        }),
        new HttpClient(),
        NullLogger<MomoGatewayService>.Instance);

    private static string HmacSha256(string key, string input)
    {
        var builder = new StringBuilder();
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
        foreach (var @byte in hmac.ComputeHash(Encoding.UTF8.GetBytes(input)))
            builder.Append(@byte.ToString("x2"));
        return builder.ToString();
    }

    private static MomoIpnPayload ValidPayload(long amount = 100000)
    {
        var payload = new MomoIpnPayload
        {
            PartnerCode = PartnerCode,
            OrderId = "PAY-888",
            RequestId = "req-1",
            Amount = amount,
            OrderInfo = "Thanh toan don hang PAY-888",
            OrderType = "momo_wallet",
            TransId = 555555,
            ResultCode = 0,
            Message = "Successful.",
            PayType = "qr",
            ResponseTime = 1720000000000,
            ExtraData = string.Empty
        };

        var raw =
            $"accessKey={AccessKey}" +
            $"&amount={payload.Amount}" +
            $"&extraData={payload.ExtraData}" +
            $"&message={payload.Message}" +
            $"&orderId={payload.OrderId}" +
            $"&orderInfo={payload.OrderInfo}" +
            $"&orderType={payload.OrderType}" +
            $"&partnerCode={payload.PartnerCode}" +
            $"&payType={payload.PayType}" +
            $"&requestId={payload.RequestId}" +
            $"&responseTime={payload.ResponseTime}" +
            $"&resultCode={payload.ResultCode}" +
            $"&transId={payload.TransId}";
        payload.Signature = HmacSha256(SecretKey, raw);
        return payload;
    }

    #region U-P4 — MoMo signature + resultCode

    [Fact]
    public void VerifyIpnAsync_ValidSignature_Succeeds()
    {
        var sut = Build();

        var result = sut.VerifyIpnAsync(ValidPayload());

        result.IsSignatureValid.Should().BeTrue();
        result.OrderId.Should().Be("PAY-888");
        result.TransId.Should().Be("555555");
        result.ResultCode.Should().Be(0);
    }

    [Fact]
    public void VerifyIpnAsync_TamperedAmount_InvalidSignature()
    {
        var sut = Build();
        var payload = ValidPayload();

        payload.Amount = 1;

        var result = sut.VerifyIpnAsync(payload);

        result.IsSignatureValid.Should().BeFalse();
    }

    [Fact]
    public void VerifyIpnAsync_MissingSignature_ReturnsInvalidResult()
    {
        var sut = Build();
        var payload = ValidPayload();
        payload.Signature = string.Empty;

        var result = sut.VerifyIpnAsync(payload);

        result.IsSignatureValid.Should().BeFalse();
        result.ResultCode.Should().Be(-1);
        result.OrderId.Should().BeEmpty();
    }

    [Fact]
    public void VerifyIpnAsync_NonZeroResultCode_StillVerifiesButCarriesCode()
    {
        // Signature verification is independent of the business result code: a legitimately
        // signed failure callback must still verify, with the resultCode surfaced to the caller.
        var sut = Build();
        var payload = ValidPayload();
        payload.ResultCode = 9006;

        var raw =
            $"accessKey={AccessKey}" +
            $"&amount={payload.Amount}" +
            $"&extraData={payload.ExtraData}" +
            $"&message={payload.Message}" +
            $"&orderId={payload.OrderId}" +
            $"&orderInfo={payload.OrderInfo}" +
            $"&orderType={payload.OrderType}" +
            $"&partnerCode={payload.PartnerCode}" +
            $"&payType={payload.PayType}" +
            $"&requestId={payload.RequestId}" +
            $"&responseTime={payload.ResponseTime}" +
            $"&resultCode={payload.ResultCode}" +
            $"&transId={payload.TransId}";
        payload.Signature = HmacSha256(SecretKey, raw);

        var result = sut.VerifyIpnAsync(payload);

        result.IsSignatureValid.Should().BeTrue();
        result.ResultCode.Should().Be(9006);
    }

    #endregion
}