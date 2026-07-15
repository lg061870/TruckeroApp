namespace Truckero.Core.DTOs;

public class StoreClerkProfileResponse {
    public bool Success { get; set; }
    public string? Message { get; set; }
    public List<StoreClerkProfileRequest> StoreClerkProfileRequests { get; set; } = new();
    public string? ErrorCode { get; set; }
}
