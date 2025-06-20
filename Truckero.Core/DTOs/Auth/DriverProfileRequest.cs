using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Truckero.Core.DataAnnotations;
using Truckero.Core.DTOs.Trucks;
using Truckero.Core.DTOs.Onboarding;

namespace Truckero.Core.DTOs.Onboarding {
    /// <summary>
    /// DTO for driver profile onboarding. Aggregates user, driver license, trucks, and payout accounts.
    /// </summary>
    public class DriverProfileRequest {
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
        public List<TruckRequest> Trucks { get; set; } = new();

        // 💳 Optional: Payout Accounts
        public List<PayoutAccountRequest> PayoutAccounts { get; set; } = new();
    }
}
