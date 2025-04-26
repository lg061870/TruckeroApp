using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Truckero.Core.Entities;

public class DriverProfile
{
    // 🔑 Primary Key and FK to User
    [Key]
    public Guid UserId { get; set; }

    [Required]
    public string LicenseNumber { get; set; } = null!;

    [Required]
    public DateTime LicenseExpiry { get; set; }

    public bool PayoutVerified { get; set; } = false;

    public string? VehicleType { get; set; } // ✅ NEW FIELD

    // 🔁 Navigation to User (no [ForeignKey] needed)
    public User User { get; set; } = null!;

    public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
}



