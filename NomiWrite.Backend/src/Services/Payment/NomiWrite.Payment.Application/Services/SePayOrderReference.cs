using System.Numerics;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using NomiWrite.Payment.Application.DTOs;

namespace NomiWrite.Payment.Application.Services;

/// <summary>
/// Order references for SePay bank transfers.
///
/// The generic reference used by VNPay and MoMo is <c>PAY-{32 hex}</c>: 36 characters
/// and containing a hyphen. That shape cannot survive a bank transfer, because
/// VietQR truncates <c>addInfo</c> at 25 characters and SePay's payment-code extractor
/// only matches <c>[0-9A-Z]</c> runs. VietQR orders therefore get their own fixed-length
/// alphanumeric reference so it round-trips through the QR, the customer's banking app,
/// and SePay's <c>code</c> field unchanged.
///
/// Required SePay dashboard setup (Cấu hình chung → Cấu trúc mã thanh toán):
/// prefix <c>NWQ</c>, minimum suffix length 20, maximum suffix length 20, type "Số và chữ".
/// </summary>
public static partial class SePayOrderReference
{
    public const string Prefix = "NWQ";
    public const int SuffixLength = 20;

    /// <summary>23 characters, which fits inside VietQR's 25 character <c>addInfo</c> limit.</summary>
    public static readonly int TotalLength = Prefix.Length + SuffixLength;

    private const string Alphabet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const int RandomBytes = 16;

    [GeneratedRegex(@"\bNWQ[0-9A-Z]{20}\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex CurrentReferencePattern();

    /// <summary>References created by the generic generator, for orders predating this format.</summary>
    [GeneratedRegex(@"\bPAY-[0-9A-F]{32}\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex LegacyReferencePattern();

    /// <summary>
    /// Creates a cryptographically random reference: 16 random bytes reduced to
    /// 20 base-36 characters, which carries ~103 bits of entropy.
    /// </summary>
    public static string Create()
    {
        Span<byte> bytes = stackalloc byte[RandomBytes];
        RandomNumberGenerator.Fill(bytes);

        var value = new BigInteger(bytes, isUnsigned: true, isBigEndian: true);
        var divisor = new BigInteger(Alphabet.Length);

        Span<char> suffix = stackalloc char[SuffixLength];
        for (var index = SuffixLength - 1; index >= 0; index--)
        {
            value = BigInteger.DivRem(value, divisor, out var remainder);
            suffix[index] = Alphabet[(int)remainder];
        }

        return Prefix + new string(suffix);
    }

    /// <summary>
    /// Ordered, de-duplicated order reference candidates found on a webhook.
    /// SePay's own <c>code</c> extraction is tried first because it is the
    /// officially supported matching path; the raw transfer fields are scanned
    /// afterwards for transfers whose content was typed by hand or truncated.
    /// </summary>
    public static IReadOnlyList<string> ExtractCandidates(SePayWebhookDto payload)
    {
        var candidates = new List<string>(4);

        void Add(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return;

            var normalized = value.Trim().ToUpperInvariant();
            if (!candidates.Contains(normalized))
                candidates.Add(normalized);
        }

        Add(payload.Code);

        foreach (var field in new[] { payload.Code, payload.Content, payload.Description })
        {
            if (string.IsNullOrWhiteSpace(field))
                continue;

            foreach (Match match in CurrentReferencePattern().Matches(field))
                Add(match.Value);

            foreach (Match match in LegacyReferencePattern().Matches(field))
                Add(match.Value);
        }

        return candidates;
    }
}
