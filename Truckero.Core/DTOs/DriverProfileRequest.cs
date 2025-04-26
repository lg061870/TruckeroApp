namespace Truckero.Core.DTOs;

public class DriverProfileRequest
{
    public string LicenseNumber { get; set; } = null!;
    public DateTime LicenseExpiryDate { get; set; }
    public string VehicleMake { get; set; } = null!;
    public string VehicleModel { get; set; } = null!;
    public string VehiclePlateNumber { get; set; } = null!;
}