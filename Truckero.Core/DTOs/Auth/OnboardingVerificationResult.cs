namespace Truckero.Core.DTOs.Auth;

public class OnboardingVerificationResult
{
    public bool UserFound { get; set; }
    public bool ProfileFound { get; set; }
    public bool TokenFound { get; set; }
    public Guid? UserId { get; set; }
}