using System.Diagnostics.Contracts;

namespace EcoMeal.api.Models;

public class User
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Contact { get; set; }
    public float Rating { get; set; }
}