namespace EcoMeal.api.Models;

public class Order
{
    public int Id { get; set; }

    
    public int UserId { get; set; }
    public User User { get; set; }

    
    public int PackageId { get; set; }
    public required Package Package { get; set; }

    public DateTime Date { get; set; } = DateTime.Now;
    public OrderStatus Status { get; set; }
}

public enum OrderStatus
{
    New,        // noua
    Confirmed,  // confirmata
    PickedUp,   // ridicata
    Cancelled   // anulata
}
