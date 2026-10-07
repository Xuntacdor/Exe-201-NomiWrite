using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NomiWrite.Payment.Application.DTOs;
using NomiWrite.Payment.Domain.Entities;
using NomiWrite.Payment.Infrastructure.Options;

namespace NomiWrite.Payment.Infrastructure.Services;

/// <summary>
/// VietQR side of the SePay integration: authenticates SePay webhooks and builds
/// the bank-transfer checkout payload (bank account + rendered QR image).
/// </summary>
public sealed class SePayVietQrService
{
    /// <summary>Scheme SePay prefixes the API key with: <c>Authorization: Apikey {key}</c>.</summary>
    private const string AuthorizationScheme = "Apikey";

    private readonly SePaySettings _settings;
    private readonly ILogger<SePayVietQrService> _logger;

    public SePayVietQrService(IOptions<SePaySettings> settings, ILogger<SePayVietQrService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    /// <summary>
    /// False when no API key is configured. Callers must fail closed in that case:
    /// an unauthenticated webhook endpoint would let anyone credit themselves a plan.
    /// </summary>
    public bool IsConfigured => !string.IsNullOrWhiteSpace(_settings.ApiKey);

    public string? ExpectedAccountNumber =>
        string.IsNullOrWhiteSpace(_settings.AccountNumber) ? null : _settings.AccountNumber.Trim();

    /// <summary>
    /// Validates <c>Authorization: Apikey {key}</c> against the configured secret.
    /// The comparison is constant time so the endpoint does not leak the key by timing.
    /// </summary>
    public bool IsAuthorized(string? authorizationHeader)
    {
        if (!IsConfigured)
        {
            _logger.LogError(
                "SePay webhook rejected: '{Section}:{Key}' is not configured, so the request cannot be authenticated.",
                SePaySettings.SectionName,
                nameof(SePaySettings.ApiKey));
            return false;
        }

        if (string.IsNullOrWhiteSpace(authorizationHeader))
            return false;

        var expectedPrefix = AuthorizationScheme + " ";
        if (!authorizationHeader.StartsWith(expectedPrefix, StringComparison.Ordinal))
            return false;

        var presentedKey = authorizationHeader[expectedPrefix.Length..].Trim();
        if (presentedKey.Length == 0)
            return false;

        return ConstantTimeEquals(presentedKey, _settings.ApiKey.Trim());
    }

    /// <summary>
    /// Builds the VietQR checkout payload for an order. The reference is the transfer
    /// content the customer must keep, so it is returned alongside the QR image URL.
    /// </summary>
    public VietQrCheckoutDto BuildCheckout(PaymentOrder order)
    {
        if (string.IsNullOrWhiteSpace(_settings.BankId)
            || string.IsNullOrWhiteSpace(_settings.AccountNumber)
            || string.IsNullOrWhiteSpace(_settings.AccountName))
        {
            throw new InvalidOperationException(
                $"'{SePaySettings.SectionName}' must define BankId, AccountNumber and AccountName before a VietQR checkout can be created.");
        }

        var bankId = _settings.BankId.Trim();
        var accountNumber = _settings.AccountNumber.Trim();
        var accountName = _settings.AccountName.Trim();

        // VietQR addresses the bank by its numeric BIN-like code, not by name.
        // "MBBank" instead of "970422" still produces a well-formed URL that 404s,
        // which surfaces to the customer as an unscanable QR and a dead checkout.
        if (!bankId.All(char.IsAsciiDigit))
        {
            throw new InvalidOperationException(
                $"'{SePaySettings.SectionName}:BankId' must be the numeric VietQR bank code " +
                $"(e.g. 970422 for MB Bank), but was '{bankId}'.");
        }

        if (!accountNumber.All(char.IsAsciiDigit))
        {
            throw new InvalidOperationException(
                $"'{SePaySettings.SectionName}:AccountNumber' must be digits only, but was '{accountNumber}'.");
        }

        // VND has no minor unit, but a discounted order is stored as e.g. 159200.00.
        // Banking apps reject fractional dong, so round half away from zero.
        var amount = decimal.Round(order.Amount, 0, MidpointRounding.AwayFromZero);

        var qrImageUrl =
            $"{_settings.QrImageBaseUrl.TrimEnd('/')}/{bankId}-{accountNumber}-compact2.png" +
            $"?amount={amount.ToString("0.##", CultureInfo.InvariantCulture)}" +
            $"&addInfo={Uri.EscapeDataString(order.OrderReference)}" +
            $"&accountName={Uri.EscapeDataString(accountName)}";

        return new VietQrCheckoutDto
        {
            BankId = bankId,
            // Purely informational: the QR only needs the bank code, so a missing
            // bank name degrades the label rather than failing the checkout.
            BankName = _settings.BankName.Trim(),
            AccountNumber = accountNumber,
            AccountName = accountName,
            Amount = amount,
            TransferContent = order.OrderReference,
            QrImageUrl = qrImageUrl
        };
    }

    private static bool ConstantTimeEquals(string left, string right)
    {
        var leftBytes = Encoding.UTF8.GetBytes(left);
        var rightBytes = Encoding.UTF8.GetBytes(right);

        return leftBytes.Length == rightBytes.Length
            && CryptographicOperations.FixedTimeEquals(leftBytes, rightBytes);
    }
}
