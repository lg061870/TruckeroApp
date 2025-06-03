using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Truckero.Core.Entities;

public class DriverProfile
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;

    [Required]
    public string FullName { get; set; } = null!; // <-- NEW

    [Required]
    public string LicenseNumber { get; set; } = null!;

    [Required]
    public DateTime LicenseExpiry { get; set; }

    [Required]
    public string LicenseFrontUrl { get; set; } = null!; // <-- NEW

    [Required]
    public string LicenseBackUrl { get; set; } = null!; // <-- NEW

    public string? Address { get; set; } // <-- NEW, if needed

    public string? ServiceArea { get; set; } // <-- NEW, if needed

    public bool PayoutVerified { get; set; } = false;
    public string? VehicleType { get; set; }

    public string HomeBase { get; set; } = null!;
    public int ServiceRadiusKm { get; set; } = 25;
    public double Latitude { get; set; }
    public double Longitude { get; set; }


    public ICollection<Truck> Vehicles { get; set; } = new List<Truck>();
    public ICollection<PayoutAccount> PayoutAccounts { get; set; } = new List<PayoutAccount>();
}
