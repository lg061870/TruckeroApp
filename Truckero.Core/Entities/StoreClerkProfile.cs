namespace Truckero.Core.Entities;

public class StoreClerkProfile
{
    // 🔑 Primary Key and FK to User
    public Guid UserId { get; set; }

    public Guid StoreId { get; set; }
    public string CorporateEmail { get; set; } = null!;
    public bool Verified { get; set; } = false;

    // 🔁 Navigation properties
    public User User { get; set; } = null!;
    public Store Store { get; set; } = null!;
}

