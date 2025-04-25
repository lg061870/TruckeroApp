namespace Truckero.Core.Entities;

public class Vehicle
{
    public Guid Id { get; set; }
    public Guid DriverProfileId { get; set; }
    public Guid VehicleTypeId { get; set; }

    public string? LicensePlate { get; set; }
    public string? Make { get; set; }     // e.g., Ford
    public string? Model { get; set; }    // e.g., Transit
    public int Year { get; set; }

    public bool IsVerified { get; set; } = false;
    public bool IsActive { get; set; } = false; // Only one can be active per driver

    public VehicleType VehicleType { get; set; } = null!;
    public DriverProfile DriverProfile { get; set; } = null!;
}
