using Truckero.Core.Entities;

namespace Truckero.Diagnostics.Data;

public static class MockBedTypes
{
    public static readonly List<BedType> Data = new()
    {
        new BedType { Id = new Guid("00000000-0000-0000-0000-000000000601"), Name = "Open Bed" },
        new BedType { Id = new Guid("00000000-0000-0000-0000-000000000602"), Name = "Covered Bed" },
        new BedType { Id = new Guid("00000000-0000-0000-0000-000000000603"), Name = "Box Truck/Van" },
        new BedType { Id = new Guid("00000000-0000-0000-0000-000000000604"), Name = "Refrigerated" }
    };
}