using System.ComponentModel.DataAnnotations;
using Truckero.Core.DataAnnotations;

namespace Truckero.Core.DTOs.Onboarding;

/// <summary>
/// DTO for driver profile onboarding. Aggregates user + driver license details.
/// </summary>
public class DriverProfileRequest
{
    // 👤 User Identity

    [Required]
    public Guid UserId { get; set; }

    [Required]
    public string FullName { get; set; } = null!;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Required]
    [Phone(ErrorMessage = "Invalid phone number")]
    public string PhoneNumber { get; set; } = null!;

    [Required]
    [MinLength(8)]
    [StrongPassword(ErrorMessage = "Password must include uppercase, lowercase, digit, and symbol")]
    public string Password { get; set; } = null!;

    // 🪪 Driver License

    [Required]
    public string LicenseNumber { get; set; } = null!;

    [Required]
    public string LicenseFrontUrl { get; set; } = null!;

    [Required]
    public string LicenseBackUrl { get; set; } = null!;

    [Required]
    public string HomeBase { get; set; } = null!; // e.g., "Pavas, San José"

    [Range(1, 100, ErrorMessage = "Radius must be between 1 and 100 km")]
    public int ServiceRadiusKm { get; set; } = 25;
    public double Latitude { get; set; }
    public double Longitude { get; set; }

    // 🚚 Optional: Trucks (can be added post-registration)

    public List<TruckDto> Trucks { get; set; } = new();
    public List<PayoutAccountDto> PayoutAccounts { get; set; } = new();

    public class TruckDto
    {
        [Required]
        public string LicensePlate { get; set; } = null!;

        [Required]
        public Guid TruckMakeId { get; set; } // Use the ID, not the name

        [Required]
        public Guid TruckTypeId { get; set; } // Use the ID, not the name

        [Required]
        public Guid TruckModelId { get; set; } // Use the ID, not the name

        [Required]
        public int Year { get; set; }
    }
}
