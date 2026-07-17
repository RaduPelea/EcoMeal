using System.ComponentModel.DataAnnotations;

namespace EcoMeal.api.Application.Models;

public class ReviewAddDTO
{
    public int BusinessId { get; set; }

    [Range(1, 5)]
    public int Stars { get; set; }

    [MaxLength(1000)]
    public string? Comment { get; set; }
}
