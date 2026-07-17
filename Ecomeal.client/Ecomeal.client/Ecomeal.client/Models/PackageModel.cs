namespace Ecomeal.client.Models;

// Client mirror of PackageDTO
public class PackageModel
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string? ImageUrl { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public decimal? OriginalPrice { get; set; }
    public int? DiscountPercent { get; set; }
    public int? DiscountHoursBeforeEnd { get; set; }
    public List<string> Images { get; set; } = new();
    public DateTime StartPickup { get; set; }
    public DateTime EndPickup { get; set; }
    public int PackageTypeId { get; set; }
    public string PackageTypeName { get; set; } = "";
}
