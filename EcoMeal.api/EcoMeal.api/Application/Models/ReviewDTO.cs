namespace EcoMeal.api.Application.Models;

public class ReviewDTO
{
    public int Id { get; set; }
    public string? UserName { get; set; }
    public int Stars { get; set; }
    public string? Comment { get; set; }
    public DateTime Date { get; set; }
}
