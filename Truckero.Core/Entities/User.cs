using Truckero.Core.Enums;

namespace Truckero.Core.Entities;

public class User
{
    public Guid Id { get; set; }

    public string Email { get; set; } = null!;
    public string? PhoneNumber { get; set; }

    // 🔗 Role assignment (defaults to Guest via seeding logic)
    public Guid RoleId { get; set; }
    public Role Role { get; set; } = null!;

    // 🪪 Identity + Federation
    public string B2CObjectId { get; set; } = null!;
    public string AuthProvider { get; set; } = "local";  // 'local', 'google', etc.
    public string? ProviderSubjectId { get; set; }

    public bool EmailVerified { get; set; } = false;
    public DateTime? LastLoginAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // 🔒 Soft lockout
    public bool IsActive { get; set; } = true;

    // 🧾 Navigation properties
    public ICollection<AuthToken> AuthTokens { get; set; } = new List<AuthToken>();
    public OnboardingProgress Onboarding { get; set; } = null!;
    public ICollection<PayoutAccount> PayoutAccounts { get; set; } = new List<PayoutAccount>();
    public ICollection<PaymentMethod> PaymentMethods { get; set; } = new List<PaymentMethod>();
    // In User.cs
    public DriverProfile? DriverProfile { get; set; }
    public CustomerProfile? CustomerProfile { get; set; }
}
