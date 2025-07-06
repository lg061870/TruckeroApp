namespace Truckero.Core.DTOs.CustomerFlow;

public class FreightBidResponse {
    public bool Success { get; set; }
    public string? Message { get; set; }
    public List<FreightBidRequest> FreightBids { get; set; } = new();
    public string? ErrorCode { get; set; }
}