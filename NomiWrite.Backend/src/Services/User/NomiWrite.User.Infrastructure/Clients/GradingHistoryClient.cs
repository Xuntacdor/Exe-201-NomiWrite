using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using NomiWrite.User.Application.Interfaces;

namespace NomiWrite.User.Infrastructure.Clients;

public class GradingHistoryClient : IGradingHistoryClient
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;
    private readonly ILogger<GradingHistoryClient> _logger;

    public GradingHistoryClient(HttpClient httpClient, ILogger<GradingHistoryClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<IReadOnlyList<GradingHistoryEntry>> GetHistoryAsync(Guid userId, string? accessToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/grading/history");

        if (!string.IsNullOrWhiteSpace(accessToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }

        using var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning(
                "AI Coordinator service returned status {StatusCode} for user {UserId}.",
                (int)response.StatusCode,
                userId);

            return Array.Empty<GradingHistoryEntry>();
        }

        var payload = await response.Content.ReadFromJsonAsync<GradingHistoryResponse>(SerializerOptions);

        if (payload?.Data is null)
            return Array.Empty<GradingHistoryEntry>();

        return payload.Data
            .Select(h => new GradingHistoryEntry(h.Id, h.SubmissionId, h.OverallBand, h.CreatedAt, h.CriteriaScores ?? new Dictionary<string, decimal>()))
            .ToList();
    }

    private sealed class GradingHistoryResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("data")]
        public List<GradingHistoryItem>? Data { get; set; }
    }

    private sealed class GradingHistoryItem
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("submissionId")]
        public Guid SubmissionId { get; set; }

        [JsonPropertyName("overallBand")]
        public decimal OverallBand { get; set; }

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("criteriaScores")]
        public Dictionary<string, decimal>? CriteriaScores { get; set; }
    }
}
