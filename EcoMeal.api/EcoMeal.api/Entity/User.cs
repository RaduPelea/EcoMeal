namespace EcoMeal.api.Models;

public class User
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
    public required string Contact { get; set; }
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}

