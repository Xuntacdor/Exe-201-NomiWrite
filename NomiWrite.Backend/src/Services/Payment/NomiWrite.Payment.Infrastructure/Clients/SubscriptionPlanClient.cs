using System.Net.Http.Json;
using NomiWrite.Payment.Application.Interfaces;

namespace NomiWrite.Payment.Infrastructure.Clients;

public class SubscriptionPlanClient : ISubscriptionPlanClient
{
    private readonly HttpClient _httpClient;

    public SubscriptionPlanClient(HttpClient httpClient) => _httpClient = httpClient;

    public async Task<SubscriptionPlanPrice?> GetActivePlanAsync(Guid planId)
    {
        var plans = await _httpClient.GetFromJsonAsync<List<PlanResponse>>("/api/subscriptions/plans");
        var plan = plans?.FirstOrDefault(p => p.Id == planId);
        return plan is null ? null : new SubscriptionPlanPrice(plan.Price, plan.Currency);
    }

    private sealed class PlanResponse
    {
        public Guid Id { get; set; }
        public decimal Price { get; set; }
        public string Currency { get; set; } = "VND";
    }
}
