using System.ComponentModel.DataAnnotations;

namespace Ecomeal.client.Models;

public class PackageAddModel
{
    [Required(ErrorMessage = "Name is required")]
    public string Name { get; set; } = "";

    [Required(ErrorMessage = "Description is required")]
    public string Description { get; set; } = "";

    public string? ImageUrl { get; set; }

    // [Required] is useless on value types, use [Range]
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
    public double Price { get; set; }

    [Required]
    public DateTime StartPickup { get; set; } = DateTime.Now;

    [Required]
    public DateTime EndPickup { get; set; } = DateTime.Now;

    // 0 = not picked -> invalid
    [Range(1, int.MaxValue, ErrorMessage = "Please pick a package type")]
    public int PackageTypeId { get; set; }
}
