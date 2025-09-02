namespace Truckero.Core.DTOs.CustomerFlow;

// File: Truckero.Core.DTOs.CustomerFlow/FindDriversStatus.cs
public record class FindDriversStatusRequest(
    Guid FreightBidId,
    bool DriversFound,
    int TotalDriversFound,
    DateTime RequestTime,
    string? StatusMessage
);
