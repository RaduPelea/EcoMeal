namespace Ecomeal.client.Models;

// Client mirror of PackageDTO
public class PackageModel
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public double Price { get; set; }
    public DateTime StartPickup { get; set; }
    public DateTime EndPickup { get; set; }
    public int PackageTypeId { get; set; }
    public string PackageTypeName { get; set; } = "";
}
