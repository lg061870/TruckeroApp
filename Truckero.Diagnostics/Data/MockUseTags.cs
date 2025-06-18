using Truckero.Core.Entities;

namespace Truckero.Diagnostics.Data;

public static class MockUseTags
{
    public static readonly List<UseTag> Data = new()
    {
        new UseTag { Id = new Guid("00000000-0000-0000-0000-000000000201"), Name = "Furniture Move" },
        new UseTag { Id = new Guid("00000000-0000-0000-0000-000000000202"), Name = "Appliance Haul" },
        new UseTag { Id = new Guid("00000000-0000-0000-0000-000000000203"), Name = "Store Delivery" },
        new UseTag { Id = new Guid("00000000-0000-0000-0000-000000000204"), Name = "Junk Removal" },
        new UseTag { Id = new Guid("00000000-0000-0000-0000-000000000205"), Name = "Fragile Goods" },
        new UseTag { Id = new Guid("00000000-0000-0000-0000-000000000206"), Name = "Helper Included" }
    };
}