using EcoMeal.api.Infrastructure;

namespace EcoMeal.api.Entities;

public class Review
{
    public int Id { get; set; }

    public int BusinessId { get; set; }
    public Business? Business { get; set; }

    public int UserId { get; set; }
    public User? User { get; set; }

    public int Stars { get; set; }
    public string? Comment { get; set; }
    public DateTime Date { get; set; } = AppTime.Now;
}
