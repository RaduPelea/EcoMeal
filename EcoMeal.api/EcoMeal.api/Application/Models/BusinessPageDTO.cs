namespace EcoMeal.api.Application.Models;

public class BusinessPageDTO
{
    public List<BusinessDTO> Items { get; set; } = new();
    public int TotalCount { get; set; }
}
