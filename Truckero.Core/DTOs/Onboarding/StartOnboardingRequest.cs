namespace Truckero.Core.DTOs.Onboarding;

// 📄 File: Truckero.Core/DTOs/Onboarding/StartOnboardingRequest.cs

public class StartOnboardingRequest
{
    //public Guid UserId { get; set; }           // 🆕 Required to identify the user starting onboarding
    //public string Email { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string Role { get; set; } = null!;  // Could become enum later (Customer, Driver, etc.)
}

