using EcoMeal.api.Infrastructure;

namespace EcoMeal.api.Application.Models;

public class PackageAddDto
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string? ImageUrl { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public DateTime StartPickup { get; set; } = AppTime.Now;
    public DateTime EndPickup { get; set; } = AppTime.Now;
    public int PackageTypeId { get; set; }
    public List<string> ImageUrls { get; set; } = new();
    public int? DiscountPercent { get; set; }
    public int? DiscountHoursBeforeEnd { get; set; }
}
