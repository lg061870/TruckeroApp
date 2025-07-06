namespace Truckero.Core.DTOs.CustomerFlow;

// File: Truckero.Core.DTOs.CustomerFlow/FindDriversStatus.cs
public record class FindDriversStatus(
    Guid FreightBidId,
    bool DriversFound,
    int TotalDriversFound,
    DateTime RequestTime,
    string? StatusMessage
);
