using System.ComponentModel.DataAnnotations;

namespace Truckero.Core.Entities;

public class TruckUseTag
{
    [Required]
    public Guid TruckId { get; set; }

    [Required]
    public Guid UseTagId { get; set; }

    public Truck Truck { get; set; } = null!;
    public UseTag UseTag { get; set; } = null!;
}