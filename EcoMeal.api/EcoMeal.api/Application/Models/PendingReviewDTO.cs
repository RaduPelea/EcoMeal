namespace EcoMeal.api.Application.Models;

public class PendingReviewDTO
{
    public int OrderId { get; set; }
    public int BusinessId { get; set; }
    public string BusinessName { get; set; } = "";
    public string PackageName { get; set; } = "";
}
