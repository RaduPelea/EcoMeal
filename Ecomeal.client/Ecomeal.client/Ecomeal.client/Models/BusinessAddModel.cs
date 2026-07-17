using System.ComponentModel.DataAnnotations;

namespace Ecomeal.client.Models;

public class BusinessAddModel
{
    [Required(ErrorMessage = "Name is required")]
    public string Name { get; set; } = "";

    [Required(ErrorMessage = "Address is required")]
    public string Address { get; set; } = "";

    [Required(ErrorMessage = "City is required")]
    public string City { get; set; } = "";

    public string? ImageUrl { get; set; }

    public string? Description { get; set; }

    [Required(ErrorMessage = "Contact is required")]
    public string Contact { get; set; } = "";

    // 0 = not picked -> invalid
    [Range(1, int.MaxValue, ErrorMessage = "Please pick a business type")]
    public int BusinessTypeId { get; set; }

    [EmailAddress(ErrorMessage = "Owner email is not a valid email address")]
    public string? OwnerEmail { get; set; }
}
