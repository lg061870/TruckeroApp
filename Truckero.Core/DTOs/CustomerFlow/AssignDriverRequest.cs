namespace Truckero.Core.DTOs.CustomerFlow;

// File: Truckero.Core.DTOs.CustomerFlow/AssignDriverRequest.cs
public record class AssignDriverRequest(
    Guid FreightBidId,
    Guid DriverBidId
);
