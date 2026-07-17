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
    [Range(0.01, 1000000, ErrorMessage = "Price must be greater than 0")]
    public decimal Price { get; set; }

    [Range(1, 1000, ErrorMessage = "Quantity must be at least 1")]
    public int Quantity { get; set; } = 1;

    [Required]
    public List<string> ImageUrls { get; set; } = new();
    public int? DiscountPercent { get; set; }
    public int? DiscountHoursBeforeEnd { get; set; }
    public DateTime StartPickup { get; set; } = DateTime.Now;

    [Required]
    public DateTime EndPickup { get; set; } = DateTime.Now.AddDays(1);

    // 0 = not picked -> invalid
    [Range(1, int.MaxValue, ErrorMessage = "Please pick a package type")]
    public int PackageTypeId { get; set; }
}
