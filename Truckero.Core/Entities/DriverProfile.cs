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
    public string LicenseNumber { get; set; } = null!;

    [Required]
    public DateTime LicenseExpiry { get; set; }

    public bool PayoutVerified { get; set; } = false;
    public string? VehicleType { get; set; }

    public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
    public ICollection<PayoutAccount> PayoutAccounts { get; set; } = new List<PayoutAccount>();
}
