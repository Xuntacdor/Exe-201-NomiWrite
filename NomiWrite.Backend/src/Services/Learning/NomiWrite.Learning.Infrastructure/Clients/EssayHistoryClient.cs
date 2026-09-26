using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NomiWrite.Learning.Application.DTOs;
using NomiWrite.Learning.Application.Interfaces;
using NomiWrite.Learning.Infrastructure.Options;

namespace NomiWrite.Learning.Infrastructure.Clients;

/// <summary>
/// Fetches the compact essay-history signals for a user over REST:
/// graded criterion scores from the AICoordinator and written topics from the
/// Writing service. The caller's JWT is forwarded so downstream services
/// enforce the same authorization rules.
/// </summary>
public class EssayHistoryClient : IEssayHistoryClient
{
    private const string GradingClientName = "GradingService";
    private const string WritingClientName = "WritingService";

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<EssayHistoryClient> _logger;

    public EssayHistoryClient(
        IHttpClientFactory httpClientFactory,
        ILogger<EssayHistoryClient> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<IReadOnlyList<GradedEssaySummaryDto>> GetGradingHistoryAsync(Guid userId, string? accessToken)
    {
        // Idempotent cross-service GET feeding the study plan. Transient connection
        // failures (e.g. peer starting up / DNS churn) must not silently blank the
        // essay history, so retry a bounded number of times with short backoff.
        const int maxAttempts = 3;
        var backoffMs = 300;

        for (var attempt = 1; ; attempt++)
        {
            try
            {
                using var request = BuildRequest(HttpMethod.Get, "/api/grading/history", accessToken);
                using var response = await SendAsync(GradingClientName, request);

                if (!response.IsSuccessStatusCode)
                    throw new HttpRequestException(
                        $"Grading service returned {(int)response.StatusCode} for user {userId}.");

                var payload = await response.Content.ReadFromJsonAsync<GradingHistoryEnvelope>(SerializerOptions);
                return payload?.Data ?? new List<GradedEssaySummaryDto>();
            }
            catch (TaskCanceledException) when (attempt < maxAttempts)
            {
                await Task.Delay(backoffMs);
                backoffMs *= 2;
            }
            catch (HttpRequestException) when (attempt < maxAttempts)
            {
                await Task.Delay(backoffMs);
                backoffMs *= 2;
            }
        }
    }

    public async Task<IReadOnlyList<EssayTopicDto>> GetWrittenTopicsAsync(Guid userId, string? accessToken)
    {
        using var request = BuildRequest(HttpMethod.Get, "/api/writing/submissions", accessToken);
        using var response = await SendAsync(WritingClientName, request);

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException(
                $"Writing service returned {(int)response.StatusCode} for user {userId}.");

        var items = await response.Content.ReadFromJsonAsync<List<WritingSubmissionPayload>>(SerializerOptions);

        return items?
            .Where(i => !string.IsNullOrWhiteSpace(i.PromptTitle))
            .Select(i => new EssayTopicDto
            {
                SubmissionId = i.Id,
                Title = i.PromptTitle!.Trim()
            })
            .ToList()
            ?? new List<EssayTopicDto>();
    }

    private async Task<HttpResponseMessage> SendAsync(string clientName, HttpRequestMessage request)
    {
        var client = _httpClientFactory.CreateClient(clientName);
        return await client.SendAsync(request);
    }

    private static HttpRequestMessage BuildRequest(HttpMethod method, string path, string? accessToken)
    {
        var request = new HttpRequestMessage(method, path);

        if (!string.IsNullOrWhiteSpace(accessToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }

        return request;
    }

    private sealed class GradingHistoryEnvelope
    {
        public bool Success { get; set; }
        public List<GradedEssaySummaryDto>? Data { get; set; }
    }

    private sealed class WritingSubmissionPayload
    {
        public Guid Id { get; set; }
        public string? PromptTitle { get; set; }
    }
}