namespace Ecomeal.client.Models;

// copie date partea de client
public class BusinessModel
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Address { get; set; }
    public string City { get; set; } = "";
    public string? ImageUrl { get; set; }
    public string? Description { get; set; }
    public required string Contact { get; set; }
    public decimal Rating { get; set; }
    public required string BusinessTypeName { get; set; }
    public string Country { get; set; } = "";
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public double? DistanceKm { get; set; }
    public int? DurationMinutes { get; set; }
}
