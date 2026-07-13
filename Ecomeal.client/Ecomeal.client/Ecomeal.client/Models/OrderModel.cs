namespace Ecomeal.client.Models;

public class OrderModel
{
    public int Id { get; set; }
    public string PackageName { get; set; } = "";
    public string Status { get; set; } = "";
    public double Price { get; set; }
    public int BusinessId { get; set; }
    public string? BusinessName { get; set; }
    public DateTime Date { get; set; }
    public string? UserName { get; set; }
    public string? UserContact { get; set; }
}
