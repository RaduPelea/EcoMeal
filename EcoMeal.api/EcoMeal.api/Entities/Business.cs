namespace EcoMeal.api.Entities;

public class Business
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Address { get; set; }
    public string City { get; set; } = "";
    public string Country { get; set; } = "Romania";
    public string? ImageUrl { get; set; }
    public string? Description { get; set; }
    public required string Contact { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    public int BusinessTypeId { get; set; }
    public BusinessType? BusinessType { get; set; }

    public int? OwnerId { get; set; }
    public User? Owner { get; set; }

    public ICollection<Package> Packages { get; set; } = new List<Package>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}
