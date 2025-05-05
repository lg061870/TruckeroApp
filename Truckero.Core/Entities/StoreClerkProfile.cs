namespace Truckero.Core.Entities;

public class StoreClerkProfile
{
    // 🔑 Primary Key and FK to User
    public Guid UserId { get; set; }

    public string CorporateEmail { get; set; } = null!;
    public bool Verified { get; set; } = false;

    // 🔁 Navigation
    public User User { get; set; } = null!;

    // 🔁 Many-to-many: Clerk can belong to many Stores
    public ICollection<StoreClerkAssignment> StoreAssignments { get; set; } = new List<StoreClerkAssignment>();
}
