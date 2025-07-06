namespace Truckero.Core.DTOs.CustomerFlow;

// File: Truckero.Core.DTOs.CustomerFlow/BidHistoryItem.cs
public record class BidHistoryItem(
    Guid BidId,
    DateTime PlacedAt,
    string PickupLocation,
    string DeliveryLocation,
    string VehicleType,
    string Category,
    decimal Amount,
    string Status
);
