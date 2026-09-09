using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using NomiWrite.Payment.Application.Interfaces;

namespace NomiWrite.Payment.Infrastructure.Clients;

public class PromoCodeValidatorClient : IPromoCodeValidator
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;
    private readonly ILogger<PromoCodeValidatorClient> _logger;

    public PromoCodeValidatorClient(HttpClient httpClient, ILogger<PromoCodeValidatorClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<PromoCodeValidationResult> ValidateAsync(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return new PromoCodeValidationResult(false, null);

        try
        {
            using var response = await _httpClient.GetAsync($"/api/subscriptions/promo-codes/{Uri.EscapeDataString(code)}/validate");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Subscription service returned status {StatusCode} for promo code validation.",
                    (int)response.StatusCode);

                return new PromoCodeValidationResult(false, null);
            }

            var payload = await response.Content.ReadFromJsonAsync<PromoCodeValidationResponse>(SerializerOptions);
            if (payload is null)
                return new PromoCodeValidationResult(false, null);

            return new PromoCodeValidationResult(payload.Valid, payload.DiscountPercent);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(
                exception,
                "Failed to validate promo code '{Code}'; failing open without a discount.",
                code);

            return new PromoCodeValidationResult(false, null);
        }
    }

    private sealed class PromoCodeValidationResponse
    {
        [JsonPropertyName("valid")]
        public bool Valid { get; set; }

        [JsonPropertyName("discountPercent")]
        public int? DiscountPercent { get; set; }
    }
}
