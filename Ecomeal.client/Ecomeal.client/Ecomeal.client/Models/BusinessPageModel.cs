namespace Ecomeal.client.Models;

public class BusinessPageModel
{
    public List<BusinessModel> Items { get; set; } = new();
    public int TotalCount { get; set; }
}
