namespace EcoMeal.api.Entities;

public class Favorite
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User? User { get; set; }

    public int BusinessId { get; set; }
    public Business? Business { get; set; }
}
