using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NomiWrite.Payment.Domain.Entities;
using NomiWrite.Payment.Infrastructure.Options;

namespace NomiWrite.Payment.Infrastructure.Services;

public class MomoIpnPayload
{
    public string PartnerCode { get; set; } = string.Empty;
    public string OrderId { get; set; } = string.Empty;
    public string RequestId { get; set; } = string.Empty;
    public long Amount { get; set; }
    public string OrderInfo { get; set; } = string.Empty;
    public string OrderType { get; set; } = string.Empty;
    public long TransId { get; set; }
    public int ResultCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public string PayType { get; set; } = string.Empty;
    public long ResponseTime { get; set; }
    public string ExtraData { get; set; } = string.Empty;
    public string Signature { get; set; } = string.Empty;
}

public sealed record MomoIpnResult(
    bool IsSignatureValid,
    string OrderId,
    string RequestId,
    string TransId,
    int ResultCode,
    string Message);

public class MomoGatewayService
{
    private const string RequestType = "captureWallet";
    private const int SuccessCode = 0;
    private const string PartnerName = "NomiWrite";
    private const string Lang = "vi";

    private static readonly MomoIpnResult InvalidResult = new(
        IsSignatureValid: false,
        OrderId: string.Empty,
        RequestId: string.Empty,
        TransId: string.Empty,
        ResultCode: -1,
        Message: string.Empty);

    private readonly MomoSettings _settings;
    private readonly HttpClient _httpClient;
    private readonly ILogger<MomoGatewayService> _logger;

    public MomoGatewayService(IOptions<MomoSettings> settings, HttpClient httpClient, ILogger<MomoGatewayService> logger)
    {
        _settings = settings.Value;
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<string> CreatePaymentAsync(PaymentOrder order)
    {
        var requestId = Guid.NewGuid().ToString();
        var amount = Convert.ToInt64(order.Amount);
        var orderInfo = $"Thanh toan don hang {order.OrderReference}";

        var rawSignature =
            $"accessKey={_settings.AccessKey}" +
            $"&amount={amount}" +
            $"&extraData=" +
            $"&ipnUrl={_settings.IpnUrl}" +
            $"&orderId={order.OrderReference}" +
            $"&orderInfo={orderInfo}" +
            $"&partnerCode={_settings.PartnerCode}" +
            $"&redirectUrl={_settings.RedirectUrl}" +
            $"&requestId={requestId}" +
            $"&requestType={RequestType}";

        var signature = ComputeHmacSha256(_settings.SecretKey, rawSignature);

        var payload = new CreatePaymentRequest
        {
            PartnerCode = _settings.PartnerCode,
            PartnerName = PartnerName,
            StoreId = _settings.PartnerCode,
            RequestId = requestId,
            Amount = amount,
            OrderId = order.OrderReference,
            OrderInfo = orderInfo,
            RedirectUrl = _settings.RedirectUrl,
            IpnUrl = _settings.IpnUrl,
            Lang = Lang,
            RequestType = RequestType,
            ExtraData = string.Empty,
            Signature = signature
        };

        var response = await _httpClient.PostAsJsonAsync(_settings.Endpoint, payload);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<CreatePaymentResponse>()
            ?? throw new InvalidOperationException("MoMo returned an empty response.");

        if (result.ResultCode != SuccessCode)
        {
            _logger.LogWarning(
                "MoMo create payment failed with resultCode {ResultCode}: {Message}",
                result.ResultCode,
                result.Message);
            throw new InvalidOperationException(
                $"MoMo payment creation failed (resultCode {result.ResultCode}): {result.Message}");
        }

        if (string.IsNullOrEmpty(result.PayUrl))
        {
            throw new InvalidOperationException("MoMo returned a successful result but no payUrl.");
        }

        return result.PayUrl;
    }

    public MomoIpnResult VerifyIpnAsync(MomoIpnPayload payload)
    {
        if (string.IsNullOrEmpty(payload.Signature))
        {
            _logger.LogWarning("MoMo IPN received without a signature.");
            return InvalidResult;
        }

        var rawSignature =
            $"accessKey={_settings.AccessKey}" +
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

        var computedHash = ComputeHmacSha256(_settings.SecretKey, rawSignature);
        var isSignatureValid = ConstantTimeEquals(computedHash, payload.Signature);

        if (!isSignatureValid)
        {
            _logger.LogWarning("MoMo IPN signature verification failed for order {OrderId}.", payload.OrderId);
        }

        return new MomoIpnResult(
            IsSignatureValid: isSignatureValid,
            OrderId: payload.OrderId,
            RequestId: payload.RequestId,
            TransId: payload.TransId.ToString(),
            ResultCode: payload.ResultCode,
            Message: payload.Message);
    }

    private static bool ConstantTimeEquals(string left, string right)
    {
        var leftBytes = Encoding.UTF8.GetBytes(left);
        var rightBytes = Encoding.UTF8.GetBytes(right);

        return leftBytes.Length == rightBytes.Length
            && CryptographicOperations.FixedTimeEquals(leftBytes, rightBytes);
    }

    private static string ComputeHmacSha256(string secretKey, string rawData)
    {
        var keyBytes = Encoding.UTF8.GetBytes(secretKey);
        var dataBytes = Encoding.UTF8.GetBytes(rawData);

        using var hmac = new HMACSHA256(keyBytes);
        var hashBytes = hmac.ComputeHash(dataBytes);

        var builder = new StringBuilder(hashBytes.Length * 2);
        foreach (var @byte in hashBytes)
        {
            builder.Append(@byte.ToString("x2"));
        }

        return builder.ToString();
    }

    private sealed class CreatePaymentRequest
    {
        [JsonPropertyName("partnerCode")]
        public string PartnerCode { get; set; } = string.Empty;

        [JsonPropertyName("partnerName")]
        public string PartnerName { get; set; } = string.Empty;

        [JsonPropertyName("storeId")]
        public string StoreId { get; set; } = string.Empty;

        [JsonPropertyName("requestId")]
        public string RequestId { get; set; } = string.Empty;

        [JsonPropertyName("amount")]
        public long Amount { get; set; }

        [JsonPropertyName("orderId")]
        public string OrderId { get; set; } = string.Empty;

        [JsonPropertyName("orderInfo")]
        public string OrderInfo { get; set; } = string.Empty;

        [JsonPropertyName("redirectUrl")]
        public string RedirectUrl { get; set; } = string.Empty;

        [JsonPropertyName("ipnUrl")]
        public string IpnUrl { get; set; } = string.Empty;

        [JsonPropertyName("lang")]
        public string Lang { get; set; } = string.Empty;

        [JsonPropertyName("requestType")]
        public string RequestType { get; set; } = string.Empty;

        [JsonPropertyName("extraData")]
        public string ExtraData { get; set; } = string.Empty;

        [JsonPropertyName("signature")]
        public string Signature { get; set; } = string.Empty;
    }

    private sealed class CreatePaymentResponse
    {
        [JsonPropertyName("resultCode")]
        public int ResultCode { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("payUrl")]
        public string? PayUrl { get; set; }
    }
}
