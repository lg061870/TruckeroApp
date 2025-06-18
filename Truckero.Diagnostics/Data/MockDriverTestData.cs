using System;
using System.Collections.Generic;
using Truckero.Core.Entities;

namespace Truckero.Diagnostics.Data;

/// <summary>
/// Provides mock driver data objects for unit tests
/// </summary>
public static class MockDriverTestData {
    // Common IDs
    public static class Ids {
        public static readonly Guid ValidDriverId = Guid.Parse("b7a9c8d1-f4e3-42a6-8b0d-9f1e2c3b4a5d");
        public static readonly Guid ValidCustomerId = Guid.Parse("5f6e7d8c-9b0a-1234-5678-9f1e2c3b4a5d");
        public static readonly Guid ValidTruckId = Guid.Parse("e1d2c3b4-a5f6-47e8-9d0c-1b2a3c4d5e6f");
    }

    // Sample driver profiles - removed FullName and Address properties
    public static DriverProfile StandardDriver => new() {
        Id = Ids.ValidDriverId,
        UserId = MockUserTestData.Ids.DriverUserId,
        LicenseNumber = "D12345678",
        LicenseExpiry = DateTime.Now.AddYears(2),
        LicenseFrontUrl = "https://example.com/license-front.jpg",
        LicenseBackUrl = "https://example.com/license-back.jpg",
        ServiceArea = "Dallas County",
        PayoutVerified = false,
        HomeBase = "Dallas, TX",
        ServiceRadiusKm = 25,
        Latitude = 32.7767,
        Longitude = -96.7970
    };

    public static DriverProfile InactiveDriver => new() {
        Id = Guid.NewGuid(),
        UserId = MockUserTestData.Ids.InactiveUserId,
        LicenseNumber = "D98765432",
        LicenseExpiry = DateTime.Now.AddMonths(-3), // Expired license
        LicenseFrontUrl = "https://example.com/expired-license-front.jpg",
        LicenseBackUrl = "https://example.com/expired-license-back.jpg",
        ServiceArea = "New York County",
        PayoutVerified = true,
        HomeBase = "New York, NY",
        ServiceRadiusKm = 30,
        Latitude = 40.7128,
        Longitude = -74.0060
    };

    // Helper to create a driver with vehicles - using existing trucks from MockTruckTestData
    public static DriverProfile CreateDriverWithVehicles() {
        var driver = new DriverProfile {
            Id = Guid.NewGuid(),
            UserId = MockUserTestData.Ids.DriverUserId,
            LicenseNumber = "D55443322",
            LicenseExpiry = DateTime.Now.AddYears(2),
            LicenseFrontUrl = "https://example.com/hauler-license-front.jpg",
            LicenseBackUrl = "https://example.com/hauler-license-back.jpg",
            ServiceArea = "King County",
            PayoutVerified = true,
            HomeBase = "Seattle, WA",
            ServiceRadiusKm = 40,
            Latitude = 47.6062,
            Longitude = -122.3321,
            Trucks = new List<Truck>()
        };

        // Use existing trucks from MockTruckTestData
        var truck1 = CloneTruck(MockTruckTestData.ToyotaTundra);
        truck1.DriverProfileId = driver.Id;

        var truck2 = CloneTruck(MockTruckTestData.FordTransit);
        truck2.DriverProfileId = driver.Id;

        driver.Trucks.Add(truck1);
        driver.Trucks.Add(truck2);

        return driver;
    }

    // User associated with driver - reference the existing one from MockUserTestData
    public static User DriverUser => MockUserTestData.DriverUser;

    // Helper method to clone a truck to avoid sharing references
    private static Truck CloneTruck(Truck source) {
        return new Truck {
            Id = Guid.NewGuid(), // Always generate a new ID for the clone
            DriverProfileId = source.DriverProfileId,
            TruckTypeId = source.TruckTypeId,
            TruckMakeId = source.TruckMakeId,
            TruckModelId = source.TruckModelId,
            LicensePlate = source.LicensePlate,
            Year = source.Year,
            PhotoFrontUrl = source.PhotoFrontUrl,
            PhotoBackUrl = source.PhotoBackUrl,
            PhotoLeftUrl = source.PhotoLeftUrl,
            PhotoRightUrl = source.PhotoRightUrl,
            TruckCategoryId = source.TruckCategoryId,
            BedTypeId = source.BedTypeId,
            OwnershipType = source.OwnershipType,
            InsuranceProvider = source.InsuranceProvider,
            PolicyNumber = source.PolicyNumber,
            InsuranceDocumentUrl = source.InsuranceDocumentUrl,
            IsVerified = source.IsVerified,
            IsActive = source.IsActive
        };
    }
}