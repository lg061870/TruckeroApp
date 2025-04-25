namespace Truckero.Core.DTOs.Onboarding;

public class StartOnboardingRequest
{
    public string Email { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string Role { get; set; } = null!; // Enum later?
}
