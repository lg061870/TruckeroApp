namespace Truckero.Core.DTOs.CustomerFlow;

public class FreightBidDetailsResponse {
    public Guid FreightBidId { get; set; }
    public string PickupLocation { get; set; } = string.Empty;
    public string DeliveryLocation { get; set; } = string.Empty;
    public string VehicleType { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal? EstimatedCostMin { get; set; }
    public decimal? EstimatedCostMax { get; set; }
    public string? Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<DriverBidResponse> DriverBids { get; set; } = new();
}
