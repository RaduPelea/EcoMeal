namespace Ecomeal.client.Models;

public class EndingSoonModel
{
    public int PackageId { get; set; }
    public string Name { get; set; } = "";
    public string? ImageUrl { get; set; }
    public decimal Price { get; set; }
    public decimal? OriginalPrice { get; set; }
    public int Quantity { get; set; }
    public DateTime EndPickup { get; set; }
    public int BusinessId { get; set; }
    public string BusinessName { get; set; } = "";
    public string City { get; set; } = "";
}
