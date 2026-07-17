using EcoMeal.api.Infrastructure;

namespace EcoMeal.api.Entities;

public class CustomerReview
{
    public int Id { get; set; }

    public int OrderId { get; set; }
    public Order? Order { get; set; }

    public int Stars { get; set; }
    public string? Comment { get; set; }
    public DateTime Date { get; set; } = AppTime.Now;
}
