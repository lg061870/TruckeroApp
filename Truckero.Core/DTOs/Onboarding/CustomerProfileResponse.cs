namespace Truckero.Core.DTOs.Onboarding;

public class CustomerProfileResponse {
    public bool Success { get; set; }
    public string? Message { get; set; }
    public List<CustomerProfileRequest> CustomerProfiles { get; set; } = new();
    public string? ErrorCode { get; set; }
}