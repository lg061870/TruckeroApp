using System.ComponentModel.DataAnnotations;
using Truckero.Core.DataAnnotations;
using Truckero.Core.Entities;

public class DriverProfileRequest
{
    [Required]
    public string FullName { get; set; } = null!;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Required]
    [Phone(ErrorMessage = "Invalid phone number")]
    public string PhoneNumber { get; set; } = null!;

    [Required]
    public string LicenseNumber { get; set; } = null!;

    [Required]
    [MinLength(8)]
    [StrongPassword(ErrorMessage = "Password must include uppercase, lowercase, digit, and symbol")] // add this if available
    public string Password { get; set; } = null!;

    [Required]
    public string UserId { get; set; } = null!;

    [Required]
    public string LicenseFrontUrl { get; set; } = null!;

    [Required]
    public string LicenseBackUrl { get; set; } = null!;

    // Optional: add Address, ServiceArea, Payment if required:
    // [Required]
    // [StringLength(200, MinimumLength = 5, ErrorMessage = "Address must be between 5 and 200 characters")]
    // public string Address { get; set; } = null!;

    // ... other fields ...
    public List<TruckDto> Trucks { get; set; } = new();

    public class TruckDto
    {
        [Required]
        public string LicensePlate { get; set; } = null!;
        [Required]
        public string Make { get; set; } = null!;
        [Required]
        public string Model { get; set; } = null!;
        [Required]
        public string Year { get; set; } = null!;
    }

}
