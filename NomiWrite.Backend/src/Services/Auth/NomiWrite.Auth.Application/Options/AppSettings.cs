namespace NomiWrite.Auth.Application.Options;

public class AppSettings
{
    public const string SectionName = "AppSettings";

    public string FrontendBaseUrl { get; set; } = "http://localhost:3000";
}