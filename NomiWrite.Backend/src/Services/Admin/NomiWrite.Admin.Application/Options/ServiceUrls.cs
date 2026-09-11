namespace NomiWrite.Admin.Application.Options;

public class ServiceUrls
{
    public const string SectionName = "ServiceUrls";

    public string AuthService { get; set; } = string.Empty;
    public string WritingService { get; set; } = string.Empty;
    public string PaymentService { get; set; } = string.Empty;
    public string SubscriptionService { get; set; } = string.Empty;
}
