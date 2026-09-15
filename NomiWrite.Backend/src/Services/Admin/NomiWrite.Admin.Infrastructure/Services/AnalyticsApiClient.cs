namespace NomiWrite.Admin.Infrastructure.Services;

public class AnalyticsApiClient
{
    public HttpClient Client { get; }

    public AnalyticsApiClient(HttpClient httpClient)
    {
        Client = httpClient;
    }
}