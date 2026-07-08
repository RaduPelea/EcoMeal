using System.ComponentModel.DataAnnotations;

namespace Ecomeal.client.Models;

public class PackageAddModel
{
    [Required]
    public string Name { get; set; } = "";
    [Required]
    public string Description { get; set; } = "";
    [Required]
    public double Price { get; set; }
    [Required]
    public DateTime StartPickup { get; set; } = DateTime.Now;
    [Required]
    public DateTime EndPickup { get; set; } = DateTime.Now;
    [Required]
    public int PackageTypeId { get; set; }
}
