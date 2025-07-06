namespace Truckero.Core.DTOs.CustomerFlow;

public record class FreightBidDetailsRequest(
    Guid FreightBidId,
    string PickupLocation,
    string DeliveryLocation,
    string VehicleType,
    string Category,
    decimal? EstimatedCostMin,
    decimal? EstimatedCostMax,
    string? Status,
    DateTime CreatedAt,
    List<DriverBidResponse> DriverBids
);
