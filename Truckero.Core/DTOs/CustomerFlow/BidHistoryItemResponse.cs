namespace Truckero.Core.DTOs.CustomerFlow;

public class BidHistoryItemResponse {
    public Guid BidId { get; set; }
    public DateTime PlacedAt { get; set; }
    public string PickupLocation { get; set; } = string.Empty;
    public string DeliveryLocation { get; set; } = string.Empty;
    public string VehicleType { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
}