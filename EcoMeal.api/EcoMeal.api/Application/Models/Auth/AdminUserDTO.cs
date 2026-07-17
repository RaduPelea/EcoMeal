namespace EcoMeal.api.Application.Models.Auth;

public class AdminUserDTO
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public bool EmailConfirmed { get; set; }
    public bool IsBusiness { get; set; }
    public string? BusinessName { get; set; }
    public int LocationCount { get; set; }
}
