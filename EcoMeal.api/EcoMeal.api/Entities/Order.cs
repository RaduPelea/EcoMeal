using Microsoft.EntityFrameworkCore;

using EcoMeal.api.Infrastructure;

namespace EcoMeal.api.Entities;

public class Order
{
    public int Id { get; set; }

    
    public int UserId { get; set; }
    public User? User { get; set; }


    public int PackageId { get; set; }
    public Package? Package { get; set; }

    public DateTime Date { get; set; } = AppTime.Now;
    public OrderStatus Status { get; set; }
    public int? LoyaltyDiscountPercent { get; set; }
    [Precision(18, 2)]
    public decimal Price { get; set; }
}

public enum OrderStatus
{
    New,        // noua
    Confirmed,  // confirmata
    PickedUp,   // ridicata
    Cancelled   // anulata
}
