using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Truckero.Core.DTOs.Onboarding;
using Truckero.Core.DTOs.PaymentAccount;
using Truckero.Core.DTOs.PayoutAccount;
using Truckero.Core.Entities;

namespace Truckero.Core.DTOs; 

public class UserRequest {
    [Key]
    public Guid Id { get; set; }

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    public string Email { get; set; } = null!;

    public string PhoneNumber { get; set; }

    // Common profile properties moved from CustomerProfile and DriverProfile
    [Required]
    public string FullName { get; set; } = string.Empty;

    public string Address { get; set; }

    // 🔗 Default role (used for login/routing)
    public Guid RoleId { get; set; }
    public bool EmailVerified { get; set; } = false;
    public DateTime? LastLoginAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    // 🔁 Role Profiles
    public Guid? DriverProfileId { get; set; }
    public Guid? CustomerProfileId { get; set; }
    public Guid? StoreClerkProfileId { get; set; }

    // 📦 Navigation
    public ICollection<Guid> AuthTokensIds { get; set; } = new List<Guid>();
    public Guid OnboardingIds { get; set; }
}
