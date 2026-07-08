namespace EcoMeal.api.Models;
//@INSERT INTO Businesses (Name, Address, Description, Contact, Rating, BusinessTypeId)
//VALUES (N'Numele afacerii', N'Orasul', N'Descriere', N'email@contact.ro', 4.5, 1);
public class Business
{   
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Address { get; set; }
    public string? Description { get; set; }
    public required string Contact { get; set; }
    public decimal Rating { get; set; }

    
    public int BusinessTypeId { get; set; }
    public BusinessType? BusinessType { get; set; }

    
    public ICollection<Package> Packages { get; set; } = new List<Package>();
}
