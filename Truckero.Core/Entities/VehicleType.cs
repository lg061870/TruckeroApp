using System.ComponentModel.DataAnnotations;

namespace Truckero.Core.Entities;

public class VehicleType
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = null!;

    [MaxLength(250)]
    public string? Description { get; set; }

    public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
}

