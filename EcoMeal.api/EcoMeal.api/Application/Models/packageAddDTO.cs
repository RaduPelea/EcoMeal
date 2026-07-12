namespace EcoMeal.api.Models;

public class packageAddDTO
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string? ImageUrl { get; set; }
    public double Price { get; set; }
    public DateTime StartPickup { get; set; } = DateTime.Now;
    public DateTime EndPickup { get; set; } = DateTime.Now;
    public int PackageTypeId { get; set; }
}