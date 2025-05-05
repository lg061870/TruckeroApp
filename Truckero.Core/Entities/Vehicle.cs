using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Truckero.Core.Entities;

public class Vehicle
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid DriverProfileId { get; set; }

    [Required]
    public Guid VehicleTypeId { get; set; }

    public string? LicensePlate { get; set; }
    public string? Make { get; set; }
    public string? Model { get; set; }
    public int Year { get; set; }

    public bool IsVerified { get; set; } = false;
    public bool IsActive { get; set; } = false;

    [ForeignKey(nameof(VehicleTypeId))]
    public VehicleType VehicleType { get; set; } = null!;

    [ForeignKey(nameof(DriverProfileId))]
    public DriverProfile DriverProfile { get; set; } = null!;
}
