using System.ComponentModel.DataAnnotations;

namespace Truckero.Core.Entities;

public class TruckType
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = null!;

    [MaxLength(250)]
    public string? Description { get; set; }

    public ICollection<Truck> Vehicles { get; set; } = new List<Truck>();
}

