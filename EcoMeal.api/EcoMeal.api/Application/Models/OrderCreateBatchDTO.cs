namespace EcoMeal.api.Application.Models;

public class OrderCreateBatchDTO
{
    public List<int> PackageIds { get; set; } = new();
    public bool UseLoyaltyDiscount { get; set; }
}
