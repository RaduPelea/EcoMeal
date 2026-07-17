using Microsoft.AspNetCore.Identity;

namespace EcoMeal.api.Entities;

public class User : IdentityUser<int>
{
    public string? Name { get; set; }
    public string? Contact { get; set; }
    public string? City { get; set; }
    public string Country { get; set; } = "Romania";
    public string? PreferredPackageTypes { get; set; }
    public int LoyaltyClaimedRewards { get; set; }
    public bool HasActiveLoyaltyDiscount { get; set; }
    public string? EmailVerificationCode { get; set; }
    public DateTime? EmailVerificationCodeExpiresAt { get; set; }
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}

