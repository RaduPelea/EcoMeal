namespace EcoMeal.api.Application.Models;

// dto = data transfer object

public class BusinessDTO
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
}
