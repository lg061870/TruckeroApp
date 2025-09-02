using System.ComponentModel.DataAnnotations;

namespace Truckero.Core.Entities;

public class TruckCategory
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    public string? Icon { get; set; }
}
