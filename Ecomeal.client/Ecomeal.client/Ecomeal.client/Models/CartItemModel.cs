namespace Ecomeal.client.Models;

public class CartItemModel
{
    public int PackageId { get; set; }
    public string Name { get; set; } = "";
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
    public int BusinessId { get; set; }
    public string BusinessName { get; set; } = "";
}
