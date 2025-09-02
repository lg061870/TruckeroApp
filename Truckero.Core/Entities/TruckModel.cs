using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Truckero.Core.Entities;

public class TruckModel
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public Guid MakeId { get; set; }

    [ForeignKey(nameof(MakeId))]
    public TruckMake Make { get; set; } = null!;
    public string? Icon { get; set; }
}
