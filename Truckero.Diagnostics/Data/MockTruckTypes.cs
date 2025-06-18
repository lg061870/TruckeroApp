using Truckero.Core.Entities;

namespace Truckero.Diagnostics.Data;

public static class MockTruckTypes
{
    public static readonly List<TruckType> Data = new()
    {
        new TruckType { Id = new Guid("00000000-0000-0000-0000-000000000101"), Name = "Motorcycle" },
        new TruckType { Id = new Guid("00000000-0000-0000-0000-000000000102"), Name = "Sedan" },
        new TruckType { Id = new Guid("00000000-0000-0000-0000-000000000103"), Name = "SUV" },
        new TruckType { Id = new Guid("00000000-0000-0000-0000-000000000104"), Name = "Pickup" },
        new TruckType { Id = new Guid("00000000-0000-0000-0000-000000000105"), Name = "Cargo Van" },
        new TruckType { Id = new Guid("00000000-0000-0000-0000-000000000106"), Name = "Box Truck" },
        new TruckType { Id = new Guid("00000000-0000-0000-0000-000000000107"), Name = "Flatbed" },
        new TruckType { Id = new Guid("00000000-0000-0000-0000-000000000108"), Name = "Trailer" }
    };
}