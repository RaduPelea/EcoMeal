namespace EcoMeal.api.Application.Models;

// BusinessDTO + the business packages
public class BusinessDetailsDTO : BusinessDTO
{
    public List<PackageDTO> Packages { get; set; } = new();
}
