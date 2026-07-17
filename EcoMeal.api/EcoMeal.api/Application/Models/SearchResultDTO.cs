namespace EcoMeal.api.Application.Models;

public class SearchResultDTO
{
    public string Type { get; set; } = "";
    public int BusinessId { get; set; }
    public string Name { get; set; } = "";
    public string? Detail { get; set; }
    public string? ImageUrl { get; set; }
}
