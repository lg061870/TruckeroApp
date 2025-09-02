namespace Truckero.Core.DTOs.CustomerFlow;

public class FreightBidDetailsResponse {
    public bool Success { get; set; }
    public string? Message { get; set; }
    public List<FreightBidDetailsRequest> FreightBidDetails { get; set; } = new();
    public string? ErrorCode { get; set; }
}
