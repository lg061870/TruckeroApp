namespace Truckero.Core.DTOs.Onboarding;

public class VerifyCodeRequest
{
    public string Code { get; set; } = null!;
    public string Method { get; set; } = "sms"; // or "email"
}
