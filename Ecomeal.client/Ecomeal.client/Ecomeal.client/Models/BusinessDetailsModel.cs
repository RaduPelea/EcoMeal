namespace Ecomeal.client.Models;

public class BusinessDetailsModel
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Address { get; set; } = "";
    public string City { get; set; } = "";
    public string Country { get; set; } = "";
    public string? ImageUrl { get; set; }
    public string? Description { get; set; }
    public string Contact { get; set; } = "";
    public decimal Rating { get; set; }
    public string BusinessTypeName { get; set; } = "";
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public double? DistanceKm { get; set; }
    public int? DurationMinutes { get; set; }

    // business packages
    public List<PackageModel> Packages { get; set; } = new();
}
