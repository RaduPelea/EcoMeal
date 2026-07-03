using System.ComponentModel.DataAnnotations;

namespace EcoMeal.api.Models;

public class PackageType
{   
    [Key]
    public int Id { get; set; }
    [MaxLength(20)]
    public required string Name { get; set; }

    /// un  tip de pachet e folosit de mai multe pachete
    public ICollection<Package> Packages { get; set; } = new List<Package>();
}
