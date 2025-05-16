using System.ComponentModel.DataAnnotations;

namespace Truckero.Core.Entities
{
    public class User
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        public string Email { get; set; } = null!;

        public string? PhoneNumber { get; set; }

        // 🔗 Default role (used for login/routing)
        public Guid RoleId { get; set; }
        public Role Role { get; set; } = null!;

        public bool EmailVerified { get; set; } = false;
        public DateTime? LastLoginAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        // 🔁 Role Profiles
        public DriverProfile? DriverProfile { get; set; }
        public CustomerProfile? CustomerProfile { get; set; }
        public StoreClerkProfile? StoreClerkProfile { get; set; }

        // 📦 Navigation
        public ICollection<AuthToken> AuthTokens { get; set; } = new List<AuthToken>();
        public OnboardingProgress Onboarding { get; set; } = null!;
        public ICollection<PayoutAccount> PayoutAccounts { get; set; } = new List<PayoutAccount>();
        public ICollection<PaymentMethod> PaymentMethods { get; set; } = new List<PaymentMethod>();
    }
}
