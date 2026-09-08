using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using NomiWrite.User.Application.Interfaces;

namespace NomiWrite.User.Infrastructure.Clients;

public class WritingHistoryClient : IWritingHistoryClient
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;
    private readonly ILogger<WritingHistoryClient> _logger;

    public WritingHistoryClient(HttpClient httpClient, ILogger<WritingHistoryClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<IReadOnlyList<WritingSubmissionSummary>> GetSubmissionsAsync(Guid userId, string? accessToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/writing/submissions");

        if (!string.IsNullOrWhiteSpace(accessToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }

        using var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning(
                "Writing service returned status {StatusCode} for user {UserId}.",
                (int)response.StatusCode,
                userId);

            return Array.Empty<WritingSubmissionSummary>();
        }

        var payload = await response.Content.ReadFromJsonAsync<SubmissionItem[]>(SerializerOptions)
            ?? Array.Empty<SubmissionItem>();

        return payload
            .Select(s => new WritingSubmissionSummary(s.Id, s.SubmittedAt.GetValueOrDefault(), s.Status))
            .ToList();
    }

    private sealed class SubmissionItem
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("submittedAt")]
        public DateTime? SubmittedAt { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;
    }
}
