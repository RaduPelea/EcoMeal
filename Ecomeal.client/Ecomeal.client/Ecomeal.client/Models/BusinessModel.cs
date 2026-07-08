namespace Ecomeal.client.Models;

// copie date partea de client
public class BusinessModel
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Address { get; set; }
    public string? Description { get; set; }
    public required string Contact { get; set; }
    public decimal Rating { get; set; }
    public required string BusinessTypeName { get; set; }
}
