using Truckero.Core.Enums;

namespace Truckero.Core.Entities;

public class Role
{
    public Guid Id { get; set; }

    // 🔄 Change from Enum to string
    public string? Name { get; set; }

    public string? Description { get; set; }

    public ICollection<User> Users { get; set; } = new List<User>();
}

