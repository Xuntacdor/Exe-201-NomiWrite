using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NomiWrite.Admin.Application.DTOs;
using NomiWrite.Admin.Application.Interfaces;
using NomiWrite.Admin.Application.Options;
using NomiWrite.Admin.Infrastructure.Options;

namespace NomiWrite.Admin.Infrastructure.Services;

public class AnalyticsAggregatorService : IAnalyticsService
{
    private readonly AnalyticsApiClient _apiClient;
    private readonly ServiceUrls _serviceUrls;
    private readonly AnalyticsSettings _settings;
    private readonly IMemoryCache _cache;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AnalyticsAggregatorService> _logger;

    private const string OverviewCacheKey = "admin_analytics_overview";

    public AnalyticsAggregatorService(
        AnalyticsApiClient apiClient,
        IOptions<ServiceUrls> serviceUrls,
        IOptions<AnalyticsSettings> settings,
        IMemoryCache cache,
        IHttpContextAccessor httpContextAccessor,
        ILogger<AnalyticsAggregatorService> logger)
    {
        _apiClient = apiClient;
        _serviceUrls = serviceUrls.Value;
        _settings = settings.Value;
        _cache = cache;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task<AnalyticsOverviewDto> GetOverviewAsync()
    {
        if (_cache.TryGetValue(OverviewCacheKey, out AnalyticsOverviewDto? cached) && cached is not null)
            return cached;

        var overview = await BuildOverviewAsync();

        _cache.Set(OverviewCacheKey, overview, TimeSpan.FromMinutes(Math.Clamp(_settings.CacheTtlMinutes, 1, 60)));

        return overview;
    }

    private async Task<AnalyticsOverviewDto> BuildOverviewAsync()
    {
        var overview = new AnalyticsOverviewDto { GeneratedAt = DateTime.UtcNow };

        var tasks = new[]
        {
            FetchAuthMetricsAsync(overview),
            FetchWritingMetricsAsync(overview),
            FetchPaymentMetricsAsync(overview),
            FetchSubscriptionMetricsAsync(overview)
        };

        await Task.WhenAll(tasks);

        return overview;
    }

    private async Task FetchAuthMetricsAsync(AnalyticsOverviewDto overview)
    {
        try
        {
            using var response = await SendAdminGetAsync(
                $"{TrimBase(_serviceUrls.AuthService)}/api/admin/users/analytics",
                HttpCompletionOption.ResponseHeadersRead);

            response.EnsureSuccessStatusCode();

            var metrics = await response.Content.ReadFromJsonAsync<AuthMetrics>();
            if (metrics is not null)
            {
                overview.TotalUsers = metrics.TotalUsers;
                overview.ActiveUsers = metrics.ActiveUsers;
                overview.BannedUsers = metrics.BannedUsers;
            }
        }
        catch (Exception ex) when (IsUpstreamFailure(ex))
        {
            LogWarning(overview, "Auth", ex);
        }
    }

    private async Task FetchWritingMetricsAsync(AnalyticsOverviewDto overview)
    {
        try
        {
            using var response = await SendAdminGetAsync(
                $"{TrimBase(_serviceUrls.WritingService)}/api/admin/submissions/analytics",
                HttpCompletionOption.ResponseHeadersRead);

            response.EnsureSuccessStatusCode();

            var metrics = await response.Content.ReadFromJsonAsync<WritingMetrics>();
            if (metrics is not null)
            {
                overview.TotalSubmissions = metrics.TotalSubmissions;
                overview.GradedSubmissions = metrics.GradedSubmissions;
                overview.PendingSubmissions = metrics.PendingSubmissions;
            }
        }
        catch (Exception ex) when (IsUpstreamFailure(ex))
        {
            LogWarning(overview, "Writing", ex);
        }
    }

    private async Task FetchPaymentMetricsAsync(AnalyticsOverviewDto overview)
    {
        try
        {
            using var response = await SendAdminGetAsync(
                $"{TrimBase(_serviceUrls.PaymentService)}/api/admin/payments/analytics",
                HttpCompletionOption.ResponseHeadersRead);

            response.EnsureSuccessStatusCode();

            var metrics = await response.Content.ReadFromJsonAsync<PaymentMetrics>();
            if (metrics is not null)
            {
                overview.TotalRevenue = metrics.TotalRevenue;
                overview.CompletedTransactions = metrics.CompletedTransactions;
            }
        }
        catch (Exception ex) when (IsUpstreamFailure(ex))
        {
            LogWarning(overview, "Payment", ex);
        }
    }

    private async Task FetchSubscriptionMetricsAsync(AnalyticsOverviewDto overview)
    {
        try
        {
            using var response = await SendAdminGetAsync(
                $"{TrimBase(_serviceUrls.SubscriptionService)}/api/admin/subscriptions/analytics",
                HttpCompletionOption.ResponseHeadersRead);

            response.EnsureSuccessStatusCode();

            var metrics = await response.Content.ReadFromJsonAsync<SubscriptionMetrics>();
            if (metrics is not null)
                overview.ActiveVipMembers = metrics.ActiveVipMembers;
        }
        catch (Exception ex) when (IsUpstreamFailure(ex))
        {
            LogWarning(overview, "Subscription", ex);
        }
    }

    private async Task<HttpResponseMessage> SendAdminGetAsync(string url, HttpCompletionOption completionOption)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, url);

        var callersToken = _httpContextAccessor.HttpContext?.Request.Headers.Authorization.ToString();
        if (!string.IsNullOrEmpty(callersToken))
        {
            request.Headers.TryAddWithoutValidation("Authorization", callersToken);
        }

        return await _apiClient.Client.SendAsync(request, completionOption);
    }

    private void LogWarning(AnalyticsOverviewDto overview, string service, Exception exception)
    {
        _logger.LogWarning(
            "Failed to fetch {Service} metrics for admin analytics: {Message}",
            service,
            exception.Message);

        overview.Warnings.Add($"Unable to retrieve metrics from {service} service.");
    }

    private static bool IsUpstreamFailure(Exception exception) =>
        exception is HttpRequestException
            or TaskCanceledException
            or JsonException
            or TimeoutException;

    private static string TrimBase(string baseUrl) => baseUrl.TrimEnd('/');

    private sealed class AuthMetrics
    {
        [JsonPropertyName("totalUsers")] public int TotalUsers { get; set; }
        [JsonPropertyName("activeUsers")] public int ActiveUsers { get; set; }
        [JsonPropertyName("bannedUsers")] public int BannedUsers { get; set; }
    }

    private sealed class WritingMetrics
    {
        [JsonPropertyName("totalSubmissions")] public int TotalSubmissions { get; set; }
        [JsonPropertyName("gradedSubmissions")] public int GradedSubmissions { get; set; }
        [JsonPropertyName("pendingSubmissions")] public int PendingSubmissions { get; set; }
    }

    private sealed class PaymentMetrics
    {
        [JsonPropertyName("totalRevenue")] public decimal TotalRevenue { get; set; }
        [JsonPropertyName("completedTransactions")] public int CompletedTransactions { get; set; }
    }

    private sealed class SubscriptionMetrics
    {
        [JsonPropertyName("activeVipMembers")] public int ActiveVipMembers { get; set; }
    }
}