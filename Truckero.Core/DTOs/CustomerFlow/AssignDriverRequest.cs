namespace Truckero.Core.DTOs.CustomerFlow;

// For one assignment per response, but keeping your "list" convention
public class AssignDriverRequest {
    public Guid FreightBidId { get; set; }
    public Guid DriverBidId { get; set; }
    public Guid? DriverId { get; set; } // Optional: if your system relates these
    public DateTime? AssignmentTime { get; set; }
}