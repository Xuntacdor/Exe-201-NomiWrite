using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using NomiWrite.Writing.Application.Interfaces;

namespace NomiWrite.Writing.Infrastructure.Clients;

public class SubscriptionServiceClient : ISubscriptionStatusClient
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;
    private readonly ILogger<SubscriptionServiceClient> _logger;

    public SubscriptionServiceClient(HttpClient httpClient, ILogger<SubscriptionServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<SubscriptionStatusResult> GetCurrentSubscriptionAsync(Guid userId, string? accessToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/subscriptions/me");

        if (!string.IsNullOrWhiteSpace(accessToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }

        using var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning(
                "Subscription service returned status {StatusCode} for user {UserId}.",
                (int)response.StatusCode,
                userId);

            return new SubscriptionStatusResult(false, null, null);
        }

        var payload = await response.Content.ReadFromJsonAsync<SubscriptionStatusResponse>(SerializerOptions);

        if (payload?.Subscription is null)
            return new SubscriptionStatusResult(false, null, null);

        var hasActiveSubscription = string.Equals(
            payload.Subscription.Status,
            "Active",
            StringComparison.OrdinalIgnoreCase);

        return new SubscriptionStatusResult(
            hasActiveSubscription,
            payload.Subscription.PlanName,
            payload.Subscription.EndDate);
    }

    private sealed class SubscriptionStatusResponse
    {
        [JsonPropertyName("hasSubscription")]
        public bool HasSubscription { get; set; }

        [JsonPropertyName("status")]
        public SubscriptionStatusPayload? Subscription { get; set; }
    }

    private sealed class SubscriptionStatusPayload
    {
        [JsonPropertyName("planName")]
        public string PlanName { get; set; } = string.Empty;

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("endDate")]
        public DateTime EndDate { get; set; }
    }
}