namespace NomiWrite.Payment.Infrastructure.Options;

public class VnPaySettings
{
    public const string SectionName = "VnPaySettings";

    public string TmnCode { get; set; } = string.Empty;
    public string HashSecret { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public string ReturnUrl { get; set; } = string.Empty;
    public string IpnUrl { get; set; } = string.Empty;
}
