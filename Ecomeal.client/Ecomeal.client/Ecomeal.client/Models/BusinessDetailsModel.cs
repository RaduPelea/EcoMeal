namespace Ecomeal.client.Models;

public class BusinessDetailsModel
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Address { get; set; } = "";
    public string? Description { get; set; }
    public string Contact { get; set; } = "";
    public string BusinessTypeName { get; set; } = "";

    // business packages
    public List<PackageModel> Packages { get; set; } = new();
}
