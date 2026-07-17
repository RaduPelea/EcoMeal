namespace Ecomeal.client.Models;

public class OrderModel
{
    public int Id { get; set; }
    public string PackageName { get; set; } = "";
    public string Status { get; set; } = "";
    public decimal Price { get; set; }
    public int BusinessId { get; set; }
    public string? BusinessName { get; set; }
    public DateTime Date { get; set; }
    public string? UserName { get; set; }
    public string? UserContact { get; set; }
    public DateTime EndPickup { get; set; }
    public int? LoyaltyDiscountPercent { get; set; }
    public decimal? CustomerRating { get; set; }
    public int CustomerRatingCount { get; set; }
    public bool IsExpired => Status is "New" or "Confirmed" && DateTime.Now > EndPickup;
}
