using System.Net;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NomiWrite.Payment.Domain.Entities;
using NomiWrite.Payment.Infrastructure.Options;

namespace NomiWrite.Payment.Infrastructure.Services;

public sealed record VnPayIpnResult(
    bool IsSignatureValid,
    string OrderReference,
    string ProviderTransactionId,
    string ResponseCode,
    string TransactionStatus,
    decimal Amount);

public class VnPayGatewayService
{
    private const string Version = "2.1.0";
    private const string Command = "pay";
    private const string Currency = "VND";
    private const string OrderType = "other";
    private const string Locale = "vn";
    private const string PaymentStatusSuccess = "00";

    private static readonly VnPayIpnResult InvalidResult = new(
        IsSignatureValid: false,
        OrderReference: string.Empty,
        ProviderTransactionId: string.Empty,
        ResponseCode: string.Empty,
        TransactionStatus: string.Empty,
        Amount: 0m);

    private readonly VnPaySettings _settings;
    private readonly ILogger<VnPayGatewayService> _logger;

    public VnPayGatewayService(IOptions<VnPaySettings> settings, ILogger<VnPayGatewayService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public Task<string> CreatePaymentAsync(PaymentOrder order, string ipAddress)
    {
        var parameters = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            ["vnp_Version"] = Version,
            ["vnp_Command"] = Command,
            ["vnp_TmnCode"] = _settings.TmnCode,
            ["vnp_Amount"] = Convert.ToInt64(order.Amount * 100).ToString(),
            ["vnp_CurrCode"] = Currency,
            ["vnp_TxnRef"] = order.OrderReference,
            ["vnp_OrderInfo"] = $"Thanh toan don hang {order.OrderReference}",
            ["vnp_OrderType"] = OrderType,
            ["vnp_Locale"] = Locale,
            ["vnp_ReturnUrl"] = _settings.ReturnUrl,
            ["vnp_IpAddr"] = ipAddress,
            ["vnp_CreateDate"] = order.CreatedAt.ToString("yyyyMMddHHmmss")
        };

        var hashInput = BuildHashInput(parameters);
        var secureHash = HmacSha512(_settings.HashSecret, hashInput);

        return Task.FromResult($"{_settings.BaseUrl}?{hashInput}&vnp_SecureHash={secureHash}");
    }

    public VnPayIpnResult VerifyIpnAsync(IDictionary<string, string> vnpParams)
    {
        var parameters = new SortedDictionary<string, string>(StringComparer.Ordinal);

        foreach (var pair in vnpParams)
        {
            if (string.Equals(pair.Key, "vnp_SecureHash", StringComparison.OrdinalIgnoreCase)
                || string.Equals(pair.Key, "vnp_SecureHashType", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (!string.IsNullOrEmpty(pair.Value))
            {
                parameters[pair.Key] = pair.Value;
            }
        }

        var receivedHash = vnpParams.FirstOrDefault(p =>
            string.Equals(p.Key, "vnp_SecureHash", StringComparison.OrdinalIgnoreCase)).Value;

        if (string.IsNullOrEmpty(receivedHash))
        {
            _logger.LogWarning("VNPay IPN received without a vnp_SecureHash.");
            return InvalidResult;
        }

        var computedHash = HmacSha512(_settings.HashSecret, BuildHashInput(parameters));

        var isSignatureValid = ConstantTimeEquals(computedHash, receivedHash);
        if (!isSignatureValid)
        {
            _logger.LogWarning("VNPay IPN signature verification failed (received {Received}).", receivedHash);
        }

        return new VnPayIpnResult(
            IsSignatureValid: isSignatureValid,
            OrderReference: GetValue(parameters, "vnp_TxnRef"),
            ProviderTransactionId: GetValue(parameters, "vnp_TransactionNo"),
            ResponseCode: GetValue(parameters, "vnp_ResponseCode"),
            TransactionStatus: GetValue(parameters, "vnp_TransactionStatus"),
            Amount: ParseAmount(GetValue(parameters, "vnp_Amount")));
    }

    public static bool IsPaymentSuccessful(VnPayIpnResult result)
    {
        return result.IsSignatureValid
            && string.Equals(result.ResponseCode, PaymentStatusSuccess, StringComparison.Ordinal)
            && string.Equals(result.TransactionStatus, PaymentStatusSuccess, StringComparison.Ordinal);
    }

    private static string BuildHashInput(SortedDictionary<string, string> parameters)
    {
        var builder = new StringBuilder();

        foreach (var pair in parameters)
        {
            builder.Append(WebUtility.UrlEncode(pair.Key));
            builder.Append('=');
            builder.Append(WebUtility.UrlEncode(pair.Value));
            builder.Append('&');
        }

        if (builder.Length > 0)
        {
            builder.Length -= 1;
        }

        return builder.ToString();
    }

    private static string HmacSha512(string key, string inputData)
    {
        var keyBytes = Encoding.UTF8.GetBytes(key);
        var inputBytes = Encoding.UTF8.GetBytes(inputData);

        var builder = new StringBuilder();
        using var hmac = new HMACSHA512(keyBytes);
        var hashBytes = hmac.ComputeHash(inputBytes);

        foreach (var @byte in hashBytes)
        {
            builder.Append(@byte.ToString("x2"));
        }

        return builder.ToString();
    }

    private static bool ConstantTimeEquals(string left, string right)
    {
        var leftBytes = Encoding.UTF8.GetBytes(left);
        var rightBytes = Encoding.UTF8.GetBytes(right);

        return leftBytes.Length == rightBytes.Length
            && CryptographicOperations.FixedTimeEquals(leftBytes, rightBytes);
    }

    private static string GetValue(IDictionary<string, string> parameters, string key)
    {
        return parameters.TryGetValue(key, out var value) ? value : string.Empty;
    }

    private static decimal ParseAmount(string? amount)
    {
        return long.TryParse(amount, out var value) ? Convert.ToDecimal(value) / 100 : 0m;
    }
}
