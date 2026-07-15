namespace Truckero.Core.DTOs;

public class StoreDeliveryJobResponse {
    public bool Success { get; set; }
    public string? Message { get; set; }
    public List<StoreDeliveryJobRequest> StoreDeliveryJobRequests { get; set; } = new();
    public string? ErrorCode { get; set; }
}
