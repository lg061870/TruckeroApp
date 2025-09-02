namespace Truckero.Core.DTOs.CustomerFlow;

public class FindDriversStatusResponse {
    public bool Success { get; set; }
    public string? Message { get; set; }
    public List<FindDriversStatusRequest> BidsStatuses { get; set; } = new();
    public string? ErrorCode { get; set; }
}