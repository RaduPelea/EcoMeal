namespace EcoMeal.api.Application.Models;

public class PendingCustomerReviewDTO
{
    public int OrderId { get; set; }
    public string CustomerName { get; set; } = "";
    public string PackageName { get; set; } = "";
}
