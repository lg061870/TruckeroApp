// Namespace: TruckeroApp.DebugTools
using Truckero.Core.Entities;
using Truckero.Core.Enums;

namespace Truckero.Diagnostics.Seeders;

public static class TestSeedHelpers
{
    public static Role SeededRole(RoleType roleType) =>
        new Role
        {
            Id = roleType switch
            {
                RoleType.Guest => Guid.Parse("00000000-0000-0000-0000-000000000001"),
                RoleType.Customer => Guid.Parse("00000000-0000-0000-0000-000000000002"),
                RoleType.Driver => Guid.Parse("00000000-0000-0000-0000-000000000003"),
                RoleType.StoreClerk => Guid.Parse("00000000-0000-0000-0000-000000000004"),
                RoleType.Admin => Guid.Parse("00000000-0000-0000-0000-000000000005"),
                _ => throw new ArgumentOutOfRangeException()
            },
            Name = roleType.ToString()
        };

    public static IEnumerable<Role> AllRoles() => Enum.GetValues<RoleType>()
    .Select(role => SeededRole(role));

}