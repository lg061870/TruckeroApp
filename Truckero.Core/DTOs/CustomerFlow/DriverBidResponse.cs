using Truckero.Core.DTOs.CustomerFlow;

public class DriverBidResponse {
    public bool Success { get; set; }
    public string? Message { get; set; }
    public List<DriverBidRequest> DriverBids { get; set; } = new();
    public string? ErrorCode { get; set; }
}