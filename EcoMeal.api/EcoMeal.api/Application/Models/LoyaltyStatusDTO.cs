namespace EcoMeal.api.Application.Models;

public class LoyaltyStatusDTO
{
    public int PickedUpOrders { get; set; }
    public int EarnedRewards { get; set; }
    public int ClaimedRewards { get; set; }
    public bool HasActiveDiscount { get; set; }
    public bool CanClaim { get; set; }
    public int OrdersUntilNextReward { get; set; }
}
