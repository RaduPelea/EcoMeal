namespace EcoMeal.api.Application.Models;

// Business type sent to the client (for the dropdown)
public class BusinessTypeDTO
{
    public int Id { get; set; }
    public required string Name { get; set; }
}
