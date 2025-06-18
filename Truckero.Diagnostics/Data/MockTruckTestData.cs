using System;
using System.Collections.Generic;
using Truckero.Core.Entities;

namespace Truckero.Diagnostics.Data;

/// <summary>
/// Provides mock truck data objects for unit tests
/// </summary>
public static class MockTruckTestData
{
    // Shared IDs to use consistently across tests
    public static class Ids
    {
        public static readonly Guid ValidDriverId = Guid.Parse("a7c53eb5-9c8d-4094-9c18-b38d7c1ad74e");
        public static readonly Guid ValidTruckTypeId = Guid.Parse("00000000-0000-0000-0000-000000000104"); // Pickup
        public static readonly Guid InvalidTruckTypeId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        public static readonly Guid PickupTruckTypeId = Guid.Parse("00000000-0000-0000-0000-000000000104"); // Pickup
        public static readonly Guid VanTruckTypeId = Guid.Parse("00000000-0000-0000-0000-000000000105"); // Cargo Van
        public static readonly Guid FlatbedTruckTypeId = Guid.Parse("00000000-0000-0000-0000-000000000107"); // Flatbed
        public static readonly Guid StandardBedTypeId = Guid.Parse("00000000-0000-0000-0000-000000000601"); // Open Bed
        public static readonly Guid FlatBedTypeId = Guid.Parse("00000000-0000-0000-0000-000000000603"); // Box Truck/Van
        public static readonly Guid UtilityTruckCategoryId = Guid.Parse("00000000-0000-0000-0000-000000000501"); // Small Load
        public static readonly Guid CommercialTruckCategoryId = Guid.Parse("00000000-0000-0000-0000-000000000502"); // Standard Load

        public static readonly Guid ToyotaMakeId = Guid.Parse("00000000-0000-0000-0000-000000000302");
        public static readonly Guid FordMakeId = Guid.Parse("00000000-0000-0000-0000-000000000301");
        public static readonly Guid ChevroletMakeId = Guid.Parse("00000000-0000-0000-0000-000000000303");
        public static readonly Guid SuzukiMakeId = Guid.Parse("00000000-0000-0000-0000-000000000308"); // Other
        public static readonly Guid GMCMakeId = Guid.Parse("00000000-0000-0000-0000-000000000305");

        public static readonly Guid TundraModelId = Guid.Parse("00000000-0000-0000-0000-000000000407"); // Other
        public static readonly Guid TransitModelId = Guid.Parse("00000000-0000-0000-0000-000000000401"); // F-150
        public static readonly Guid ColoradoModelId = Guid.Parse("00000000-0000-0000-0000-000000000403"); // Silverado
        public static readonly Guid CarryModelId = Guid.Parse("00000000-0000-0000-0000-000000000407"); // Other
        public static readonly Guid SierraModelId = Guid.Parse("00000000-0000-0000-0000-000000000405"); // Sierra
    }

    // Valid truck types for seeding
    public static TruckType PickupTruckType => new()
    {
        Id = Ids.PickupTruckTypeId,
        Name = "Pickup",
        Description = "Standard pickup truck"
    };

    public static TruckType VanTruckType => new()
    {
        Id = Ids.VanTruckTypeId,
        Name = "Van",
        Description = "Cargo van"
    };

    public static TruckType FlatbedTruckType => new()
    {
        Id = Ids.FlatbedTruckTypeId,
        Name = "Flatbed",
        Description = "Flatbed truck for large items"
    };

    // Bed types
    public static BedType StandardBed => new()
    {
        Id = Ids.StandardBedTypeId,
        Name = "Standard"
    };

    public static BedType FlatBed => new()
    {
        Id = Ids.FlatBedTypeId,
        Name = "Flatbed"
    };
    
    // Truck Categories
    public static TruckCategory UtilityTruckCategory => new()
    {
        Id = Ids.UtilityTruckCategoryId,
        Name = "Utility"
    };
    
    public static TruckCategory CommercialTruckCategory => new()
    {
        Id = Ids.CommercialTruckCategoryId,
        Name = "Commercial"
    };

    // Truck Makes
    public static TruckMake ToyotaMake => new()
    {
        Id = Ids.ToyotaMakeId,
        Name = "Toyota"
    };
    
    public static TruckMake FordMake => new()
    {
        Id = Ids.FordMakeId,
        Name = "Ford"
    };
    
    public static TruckMake ChevroletMake => new()
    {
        Id = Ids.ChevroletMakeId,
        Name = "Chevrolet"
    };
    
    public static TruckMake SuzukiMake => new()
    {
        Id = Ids.SuzukiMakeId,
        Name = "Suzuki"
    };
    
    public static TruckMake GMCMake => new()
    {
        Id = Ids.GMCMakeId,
        Name = "GMC"
    };
    
    // Truck Models
    public static TruckModel TundraModel => new()
    {
        Id = Ids.TundraModelId,
        MakeId = Ids.ToyotaMakeId,
        Name = "Tundra"
    };
    
    public static TruckModel TransitModel => new()
    {
        Id = Ids.TransitModelId,
        MakeId = Ids.FordMakeId,
        Name = "Transit"
    };
    
    public static TruckModel ColoradoModel => new()
    {
        Id = Ids.ColoradoModelId,
        MakeId = Ids.ChevroletMakeId,
        Name = "Colorado"
    };
    
    public static TruckModel CarryModel => new()
    {
        Id = Ids.CarryModelId,
        MakeId = Ids.SuzukiMakeId,
        Name = "Carry"
    };
    
    public static TruckModel SierraModel => new()
    {
        Id = Ids.SierraModelId,
        MakeId = Ids.GMCMakeId,
        Name = "Sierra"
    };

    // Sample trucks
    public static Truck ToyotaTundra => new()
    {
        Id = Guid.NewGuid(), // Generate a new ID each time for test isolation
        DriverProfileId = Ids.ValidDriverId,
        TruckTypeId = Ids.PickupTruckTypeId,
        TruckMakeId = Ids.ToyotaMakeId,
        TruckModelId = Ids.TundraModelId,
        LicensePlate = "ABC123",
        Year = 2020,
        PhotoFrontUrl = "https://example.com/photos/toyota_front.jpg",
        PhotoBackUrl = "https://example.com/photos/toyota_back.jpg",
        TruckCategoryId = Ids.UtilityTruckCategoryId,
        BedTypeId = Ids.StandardBedTypeId,
        OwnershipType = OwnershipType.Owned,
        InsuranceProvider = "SafeAuto",
        PolicyNumber = "SA-12345",
        IsActive = true
    };

    public static Truck FordTransit => new()
    {
        Id = Guid.NewGuid(),
        DriverProfileId = Ids.ValidDriverId,
        TruckTypeId = Ids.VanTruckTypeId,
        TruckMakeId = Ids.FordMakeId,
        TruckModelId = Ids.TransitModelId,
        LicensePlate = "VAN456",
        Year = 2019,
        PhotoFrontUrl = "https://example.com/photos/ford_front.jpg",
        PhotoBackUrl = "https://example.com/photos/ford_back.jpg",
        TruckCategoryId = Ids.CommercialTruckCategoryId,
        OwnershipType = OwnershipType.Leased,
        InsuranceProvider = "Progressive",
        PolicyNumber = "PRG-67890",
        IsActive = true
    };

    public static Truck ChevyColorado => new()
    {
        Id = Guid.NewGuid(),
        DriverProfileId = Ids.ValidDriverId,
        TruckTypeId = Ids.PickupTruckTypeId,
        TruckMakeId = Ids.ChevroletMakeId,
        TruckModelId = Ids.ColoradoModelId,
        LicensePlate = "TRUCK789",
        Year = 2021,
        PhotoFrontUrl = "https://example.com/photos/chevy_front.jpg",
        PhotoBackUrl = "https://example.com/photos/chevy_back.jpg",
        TruckCategoryId = Ids.UtilityTruckCategoryId,
        BedTypeId = Ids.StandardBedTypeId,
        OwnershipType = OwnershipType.Owned,
        InsuranceProvider = "Geico",
        PolicyNumber = "GC-54321",
        IsActive = true
    };

    public static Truck SuzukiFlatBed1998 => new()
    {
        Id = Guid.NewGuid(),
        DriverProfileId = Ids.ValidDriverId,
        TruckTypeId = Ids.InvalidTruckTypeId, // Invalid truck type ID for testing
        TruckMakeId = Ids.SuzukiMakeId,
        TruckModelId = Ids.CarryModelId,
        LicensePlate = "FLAT123",
        Year = 1998,
        PhotoFrontUrl = "https://example.com/photos/suzuki_front.jpg",
        PhotoBackUrl = "https://example.com/photos/suzuki_back.jpg",
        BedTypeId = Ids.FlatBedTypeId,
        OwnershipType = OwnershipType.Owned,
        InsuranceProvider = "Nationwide",
        PolicyNumber = "NW-13579",
        IsActive = true
    };

    public static Truck DuplicateLicensePlateTruck => new()
    {
        Id = Guid.NewGuid(),
        DriverProfileId = Ids.ValidDriverId,
        TruckTypeId = Ids.PickupTruckTypeId,
        TruckMakeId = Ids.GMCMakeId,
        TruckModelId = Ids.SierraModelId,
        LicensePlate = "DUPLICATE", // For license plate duplication tests
        Year = 2018,
        PhotoFrontUrl = "https://example.com/photos/gmc_front.jpg",
        PhotoBackUrl = "https://example.com/photos/gmc_back.jpg",
        OwnershipType = OwnershipType.Owned,
        InsuranceProvider = "Liberty Mutual",
        PolicyNumber = "LM-24680",
        IsActive = true
    };

    public static Truck DuplicatePlateButDifferentCase => new()
    {
        Id = Guid.NewGuid(),
        DriverProfileId = Ids.ValidDriverId,
        TruckTypeId = Ids.PickupTruckTypeId,
        TruckMakeId = Ids.GMCMakeId,
        TruckModelId = Ids.SierraModelId,
        LicensePlate = "duplicate", // Same as above but different case
        Year = 2018,
        PhotoFrontUrl = "https://example.com/photos/gmc_alt_front.jpg",
        PhotoBackUrl = "https://example.com/photos/gmc_alt_back.jpg",
        OwnershipType = OwnershipType.Owned,
        InsuranceProvider = "State Farm",
        PolicyNumber = "SF-97531",
        IsActive = true
    };

    // Test method helpers
    
    /// <summary>
    /// Creates a truck with a unique license plate (based on a GUID) to avoid conflicts
    /// </summary>
    public static Truck CreateUniqueValidTruck()
    {
        var uniqueId = Guid.NewGuid();
        return new Truck
        {
            Id = uniqueId,
            DriverProfileId = Ids.ValidDriverId,
            TruckTypeId = Ids.PickupTruckTypeId,
            TruckMakeId = Ids.ToyotaMakeId,
            TruckModelId = Ids.TundraModelId,
            LicensePlate = $"TEST-{uniqueId.ToString().Substring(0, 8)}",
            Year = 2022,
            PhotoFrontUrl = "https://example.com/photos/test_front.jpg",
            PhotoBackUrl = "https://example.com/photos/test_back.jpg",
            TruckCategoryId = Ids.UtilityTruckCategoryId,
            BedTypeId = Ids.StandardBedTypeId,
            OwnershipType = OwnershipType.Owned,
            InsuranceProvider = "AAA Insurance",
            PolicyNumber = $"AAA-{uniqueId.ToString().Substring(0, 8)}",
            IsActive = true
        };
    }
}