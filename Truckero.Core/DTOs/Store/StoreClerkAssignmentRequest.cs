namespace Truckero.Core.DTOs;

public class StoreClerkAssignmentRequest {
    public Guid StoreId { get; set; }
    public StoreRequest Store { get; set; } = null!;

    public Guid ClerkUserId { get; set; }
    public StoreClerkProfileRequest ClerkProfile { get; set; } = null!;

    // Optional metadata
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    public bool IsPrimaryStore { get; set; } = false;
}
