namespace Truckero.Core.DTOs.CustomerFlow;

public record class TruckSummary(
    Guid TruckId,
    string? TruckType,
    string? LicensePlate,
    string? Description,
    string? Color,
    int? Year
);
