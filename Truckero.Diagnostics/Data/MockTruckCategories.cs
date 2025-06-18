using Truckero.Core.Entities;

namespace Truckero.Diagnostics.Data;

public static class MockTruckCategories
{
    public static readonly List<TruckCategory> Data = new()
    {
        new TruckCategory { Id = new Guid("00000000-0000-0000-0000-000000000501"), Name = "Small Load" },
        new TruckCategory { Id = new Guid("00000000-0000-0000-0000-000000000502"), Name = "Standard Load" },
        new TruckCategory { Id = new Guid("00000000-0000-0000-0000-000000000503"), Name = "Heavy Load" },
        new TruckCategory { Id = new Guid("00000000-0000-0000-0000-000000000504"), Name = "Extra Heavy Duty" }
    };
}