using Truckero.Core.Entities;

namespace Truckero.Core.DTOs;

public class StoreClerkProfileRequest {
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string CorporateEmail { get; set; } = null!;
    public bool Verified { get; set; } = false;

    public ICollection<StoreClerkAssignmentRequest> StoreAssignments { get; set; } = new List<StoreClerkAssignmentRequest>();
    public ICollection<StoreDeliveryJobRequest> DeliveryJobs { get; set; } = new List<StoreDeliveryJobRequest>();
}
