using Microsoft.EntityFrameworkCore;

namespace EcoMeal.api.Entities;

public class Package
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public string? ImageUrl { get; set; }
    public int Quantity { get; set; }
    [Precision(18, 2)]
    public decimal Price { get; set; }
    [Precision(18, 2)]
    public decimal? OriginalPrice { get; set; }
    public int? DiscountPercent { get; set; }
    public int? DiscountHoursBeforeEnd { get; set; }
    public DateTime StartPickup { get; set; }
    public DateTime EndPickup { get; set; }

    
    public int BusinessId { get; set; }
    public Business? Business { get; set; }


    public int PackageTypeId { get; set; }
    public PackageType? PackageType { get; set; }

    
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<PackageImage> Images { get; set; } = new List<PackageImage>();
}
