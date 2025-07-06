// File: Truckero.Core.DTOs.Common/DriverSummary.cs
namespace Truckero.Core.DTOs.CustomerFlow;

public record class DriverSummary(
    Guid DriverId,
    string? Name,
    string? PhotoUrl,
    double? Rating,
    int? TotalTrips,
    string? PhoneNumber
);
