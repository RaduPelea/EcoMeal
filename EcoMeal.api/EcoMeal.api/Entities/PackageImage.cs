namespace EcoMeal.api.Entities;

public class PackageImage
{
    public int Id { get; set; }
    public required string Url { get; set; }

    public int PackageId { get; set; }
    public Package? Package { get; set; }
}
