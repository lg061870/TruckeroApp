namespace Truckero.Core.DTOs.Onboarding;

// Truckero.Core.DTOs.Onboarding.VerifyCodeRequest.cs
public class VerifyCodeRequest
{
    public Guid UserId { get; set; }  // ✅ Add this line
    public string Code { get; set; } = null!;
    public string Method { get; set; } = "sms";
}

