namespace Truckero.Core.DTOs.CustomerFlow;

public class BidHistoryResponse {
    public bool Success { get; set; }
    public string? Message { get; set; }
    public List<BidHistoryRequest> BidHistorys { get; set; } = new();
    public string? ErrorCode { get; set; }
}