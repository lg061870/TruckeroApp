using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Truckero.Core.Entities;

public class DriverProfile {
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;

    // FullName and Address properties moved to User entity

    [Required]
    public string LicenseNumber { get; set; } = null!;

    [Required]
    public DateTime LicenseExpiry { get; set; }

    [Required]
    public string LicenseFrontUrl { get; set; } = null!;

    [Required]
    public string LicenseBackUrl { get; set; } = null!;

    public string? ServiceArea { get; set; }

    public bool PayoutVerified { get; set; } = false;
    public string HomeBase { get; set; } = null!;
    public int ServiceRadiusKm { get; set; } = 25;
    public double Latitude { get; set; }
    public double Longitude { get; set; }

    public ICollection<Truck> Trucks { get; set; } = new List<Truck>();
    public ICollection<PayoutAccount> PayoutAccounts { get; set; } = new List<PayoutAccount>();

    // --- Add this: navigation to DriverBids ---
    public ICollection<DriverBid> DriverBids { get; set; } = new List<DriverBid>();
}

