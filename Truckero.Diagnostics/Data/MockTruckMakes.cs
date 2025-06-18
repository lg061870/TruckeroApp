using Truckero.Core.Entities;

namespace Truckero.Diagnostics.Data;

public static class MockTruckMakes
{
    public static readonly List<TruckMake> Data = new()
    {
        new TruckMake { Id = new Guid("00000000-0000-0000-0000-000000000301"), Name = "Ford" },
        new TruckMake { Id = new Guid("00000000-0000-0000-0000-000000000302"), Name = "Toyota" },
        new TruckMake { Id = new Guid("00000000-0000-0000-0000-000000000303"), Name = "Chevrolet" },
        new TruckMake { Id = new Guid("00000000-0000-0000-0000-000000000304"), Name = "Dodge" },
        new TruckMake { Id = new Guid("00000000-0000-0000-0000-000000000305"), Name = "GMC" },
        new TruckMake { Id = new Guid("00000000-0000-0000-0000-000000000306"), Name = "RAM" },
        new TruckMake { Id = new Guid("00000000-0000-0000-0000-000000000307"), Name = "Nissan" },
        new TruckMake { Id = new Guid("00000000-0000-0000-0000-000000000308"), Name = "Other" }
    };
}