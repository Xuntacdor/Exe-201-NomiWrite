namespace NomiWrite.Payment.Infrastructure.Options;

public class SePaySettings
{
    public const string SectionName = "SePaySettings";

    /// <summary>
    /// Shared secret SePay sends as <c>Authorization: Apikey {ApiKey}</c>.
    /// The webhook fails closed (503) while this is blank.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>VietQR bank code, e.g. <c>970436</c> (VPBank) or <c>970422</c> (MBBank).</summary>
    public string BankId { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable bank name shown to the customer so they can pick the right
    /// bank in their app. Not used to build the QR, so it is optional.
    /// </summary>
    public string BankName { get; set; } = string.Empty;

    public string AccountNumber { get; set; } = string.Empty;

    public string AccountName { get; set; } = string.Empty;

    public string QrImageBaseUrl { get; set; } = "https://img.vietqr.io/image";
}
