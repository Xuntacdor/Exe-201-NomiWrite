using System.Text.Json.Serialization;

namespace NomiWrite.Payment.Application.DTOs;

/// <summary>
/// Bank-transfer webhook body posted by SePay.
/// Spec: https://developer.sepay.vn/vi/sepay-webhooks/tich-hop-webhook
/// </summary>
public class SePayWebhookDto
{
    /// <summary>SePay transaction id. Stable across every retry and replay, so it is the dedup key.</summary>
    [JsonPropertyName("id")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public long Id { get; set; }

    [JsonPropertyName("gateway")]
    public string? Gateway { get; set; }

    /// <summary>Bank local time, <c>YYYY-MM-DD HH:mm:ss</c>.</summary>
    [JsonPropertyName("transactionDate")]
    public string? TransactionDate { get; set; }

    [JsonPropertyName("accountNumber")]
    public string? AccountNumber { get; set; }

    [JsonPropertyName("subAccount")]
    public string? SubAccount { get; set; }

    /// <summary>
    /// Payment code SePay extracted from the transfer content using the
    /// "Cấu trúc mã thanh toán" pattern configured in the SePay dashboard.
    /// Null when no active pattern matched.
    /// </summary>
    [JsonPropertyName("code")]
    public string? Code { get; set; }

    /// <summary>Raw transfer content exactly as the bank reported it.</summary>
    [JsonPropertyName("content")]
    public string? Content { get; set; }

    /// <summary><c>in</c> for money in, <c>out</c> for money out.</summary>
    [JsonPropertyName("transferType")]
    public string? TransferType { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>Transfer value in VND, always positive.</summary>
    [JsonPropertyName("transferAmount")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public long TransferAmount { get; set; }

    /// <summary>Account balance after the transfer; 0 when the bank does not report it.</summary>
    [JsonPropertyName("accumulated")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public long Accumulated { get; set; }

    [JsonPropertyName("referenceCode")]
    public string? ReferenceCode { get; set; }
}

/// <summary>
/// Bank account details plus the rendered VietQR image, returned when a
/// VietQR checkout is created so the storefront never has to hold bank config.
/// </summary>
public class VietQrCheckoutDto
{
    public string BankId { get; set; } = string.Empty;

    /// <summary>Display name of the receiving bank; empty when not configured.</summary>
    public string BankName { get; set; } = string.Empty;

    public string AccountNumber { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string TransferContent { get; set; } = string.Empty;
    public string QrImageUrl { get; set; } = string.Empty;
}
