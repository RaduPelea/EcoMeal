namespace EcoMeal.api.Models;

public class Package
{
    public int Id { get; set; }
    public string Name { get; set; }
    public required string Description { get; set; }
    public int Quantity { get; set; }          
    public double Price { get; set; }         
    public DateTime StartPickup { get; set; }  
    public DateTime EndPickup { get; set; }   

    
    public int BusinessId { get; set; }
    public Business Business { get; set; }

    
    public int PackageTypeId { get; set; }
    public PackageType PackageType { get; set; }

    
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
