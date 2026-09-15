using NomiWrite.Subscription.Domain.Enums;

namespace NomiWrite.Subscription.Application.DTOs;

public class CreatePlanRequestDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Currency { get; set; } = "VND";
    public BillingCycle BillingCycle { get; set; }
    public int DurationDays { get; set; }
    public string FeaturesJson { get; set; } = "[]";
}

public class UpdatePlanRequestDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Currency { get; set; } = "VND";
    public BillingCycle BillingCycle { get; set; }
    public int DurationDays { get; set; }
    public string FeaturesJson { get; set; } = "[]";
}

public class UpdatePlanStatusRequestDto
{
    public bool IsActive { get; set; }
}

public class AdminPlanListItemDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Currency { get; set; } = "VND";
    public BillingCycle BillingCycle { get; set; }
    public int DurationDays { get; set; }
    public bool IsActive { get; set; }
    public string FeaturesJson { get; set; } = "[]";
    public DateTime CreatedAt { get; set; }
}