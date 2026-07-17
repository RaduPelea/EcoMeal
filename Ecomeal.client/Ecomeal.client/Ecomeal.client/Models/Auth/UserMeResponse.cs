namespace Ecomeal.client.Models.Auth;

public class UserMeResponse
{
    public string? Email { get; set; }
    public string? Name { get; set; }
    public string? Contact { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? BusinessName { get; set; }
    public List<string> Roles { get; set; } = new List<string>();
    public string? PreferredPackageTypes { get; set; }
    public decimal? CustomerRating { get; set; }
    public int CustomerRatingCount { get; set; }
}
