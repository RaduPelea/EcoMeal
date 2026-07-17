using System.ComponentModel.DataAnnotations;

namespace Ecomeal.client.Models;

public class ReviewAddModel
{
    public int BusinessId { get; set; }

    [Range(1, 5, ErrorMessage = "Please pick between 1 and 5 stars")]
    public int Stars { get; set; }

    [MaxLength(1000)]
    public string? Comment { get; set; }
}
