namespace Truckero.Core.Entities;

public class StoreClerkAssignment
{
    public Guid StoreId { get; set; }
    public Store Store { get; set; } = null!;

    public Guid ClerkUserId { get; set; }
    public StoreClerkProfile ClerkProfile { get; set; } = null!;

    // Optional metadata
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    public bool IsPrimaryStore { get; set; } = false;

}
