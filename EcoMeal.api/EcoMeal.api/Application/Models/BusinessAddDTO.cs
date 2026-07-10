namespace EcoMeal.api.Application.Models;

// Business data received from the client (create/update)
public class BusinessAddDTO
{
    public string Name { get; set; } = "";
    public string Address { get; set; } = "";
    public string? Description { get; set; }
    public string Contact { get; set; } = "";
    public decimal Rating { get; set; }
    public int BusinessTypeId { get; set; }
}
