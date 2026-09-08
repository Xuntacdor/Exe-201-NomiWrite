namespace NomiWrite.User.Infrastructure.Options;

public class ServiceUrls
{
    public const string SectionName = "ServiceUrls";
    public string SubscriptionService { get; set; } = "http://localhost:5160";
    public string WritingService { get; set; } = "http://localhost:5133";
    public string AICoordinatorService { get; set; } = "http://localhost:5150";
}