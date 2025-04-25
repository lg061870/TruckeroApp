using Truckero.Core.Enums;

namespace Truckero.Core.Entities;

public class Role
{
    public Guid Id { get; set; }

    // ✅ Strongly typed enum instead of string
    public RoleType Name { get; set; } = RoleType.Guest;

    // ✅ Optional description (nullable is fine)
    public string? Description { get; set; }

    // ✅ Navigation: a role may have many users
    public ICollection<User> Users { get; set; } = new List<User>();
}
