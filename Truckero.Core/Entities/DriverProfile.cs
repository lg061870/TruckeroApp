namespace Truckero.Core.Entities;

public class DriverProfile
{
    // 🔑 Primary Key and FK to User
    public Guid UserId { get; set; }

    public string LicenseNumber { get; set; } = null!;
    public DateTime LicenseExpiry { get; set; }
    public bool PayoutVerified { get; set; } = false;

    // 🔁 Navigation to User
    public User User { get; set; } = null!;

    public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
}


