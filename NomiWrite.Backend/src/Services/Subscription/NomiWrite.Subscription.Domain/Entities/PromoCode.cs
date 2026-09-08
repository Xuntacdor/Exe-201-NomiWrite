using NomiWrite.Subscription.Domain.Common;

namespace NomiWrite.Subscription.Domain.Entities;

public class PromoCode : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public int DiscountPercent { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? ExpiresAt { get; set; }
    public int? MaxRedemptions { get; set; }
    public int TimesRedeemed { get; set; }
}
