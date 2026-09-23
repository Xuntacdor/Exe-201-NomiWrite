namespace NomiWrite.Learning.Infrastructure.Options;

/// <summary>
/// Base URLs of downstream services the Learning service calls over REST while
/// assembling a study guide. Overridden per environment (Docker compose uses
/// the service container names, e.g. http://grading-service:8080).
/// </summary>
public class ServiceUrls
{
    public const string SectionName = "ServiceUrls";
    public string GradingService { get; set; } = "http://localhost:5150";
    public string WritingService { get; set; } = "http://localhost:5133";
}