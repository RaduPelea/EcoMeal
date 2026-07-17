namespace EcoMeal.api.Application.Models;

public class MyReviewDTO
{
    public int Id { get; set; }
    public int BusinessId { get; set; }
    public string BusinessName { get; set; } = "";
    public int Stars { get; set; }
    public string? Comment { get; set; }
    public DateTime Date { get; set; }
}
