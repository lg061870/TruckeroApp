using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using System.Threading.Tasks;
using Truckero.Core.Entities;
using Truckero.Core.Enums;
using Truckero.Diagnostics.Data;
using Truckero.Diagnostics.Seeders;
using Truckero.Infrastructure.Data;
using Truckero.Infrastructure.Repositories;
using Xunit;

namespace Truckero.Infrastructure.Tests.Repositories;

public class DriverRepositoryTests : IDisposable {
    private readonly AppDbContext _dbContext;
    private readonly DriverRepository _repo;
    private readonly SqliteConnection _connection;
    // No longer need persistent fields for users; will clone per test

    public DriverRepositoryTests() {
        // Use the persisted SQLite database file created by TestTemplateBuilder
        var dbPath = Path.Combine("..", "..", "..", "..", "Truckero.Diagnostics", "CloneDBs", "TestTemplate.sqlite");

        //// Ensure the file exists, otherwise tests will fail
        //if (!File.Exists(dbPath)) {
        //    throw new FileNotFoundException($"The SQLite database file was not found at: {dbPath}. Please run TestTemplateBuilder first to create it.");
        //}

        // Connect to the file-based SQLite database
        _connection = new SqliteConnection($"DataSource={dbPath}");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        _dbContext = new AppDbContext(options);
        _repo = new DriverRepository(_dbContext);

        //// Check if we already have a seeded driver role
        //var driverRole = _dbContext.Roles.FirstOrDefault(r => r.Name == RoleType.Driver.ToString());

        //// If not, create one
        //if (driverRole == null) {
        //    driverRole = TestSeedHelpers.SeededRole(RoleType.Driver);
        //    _dbContext.Roles.Add(driverRole);
        //    _dbContext.SaveChanges();
        //}

        //// No user is seeded at construction; each test will create its own user
    }

    // Remove SeededUser property; each test will create its own user

    [Fact]
    public async Task AddAsync_And_Save_Should_Persist_DriverProfile() {
        // Arrange
        var user = TestUserCloner.CloneWithNewId(MockUserTestData.DriverUser);
        user.RoleId = _dbContext.Roles.First(r => r.Name == RoleType.Driver.ToString()).Id;
        user.Onboarding = new OnboardingProgress { UserId = user.Id, StepCurrent = "Profile", Completed = false };
        _dbContext.Users.Add(user);
        _dbContext.SaveChanges();

        var driverProfile = new DriverProfile {
            Id = Guid.NewGuid(),
            UserId = user.Id,
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

        // Act
        await _repo.AddDriverProfileAsync(driverProfile);
        await _repo.SaveDriverProfileChangesAsync();

        // Assert
        var found = await _dbContext.DriverProfiles.FirstOrDefaultAsync(d => d.UserId == driverProfile.UserId);
        Assert.NotNull(found);
        Assert.Equal(driverProfile.LicenseNumber, found!.LicenseNumber);
        Assert.Equal(driverProfile.LicenseExpiry, found.LicenseExpiry);
        Assert.Equal(driverProfile.LicenseFrontUrl, found.LicenseFrontUrl);
        Assert.Equal(driverProfile.LicenseBackUrl, found.LicenseBackUrl);
    }

    [Fact]
    public async Task GetByUserIdAsync_Should_Return_DriverProfile() {
        // Arrange

        var user = TestUserCloner.CloneWithNewId(MockUserTestData.DriverUser);
        user.RoleId = _dbContext.Roles.First(r => r.Name == RoleType.Driver.ToString()).Id;
        user.Onboarding = new OnboardingProgress { UserId = user.Id, StepCurrent = "Profile", Completed = false };
        _dbContext.Users.Add(user);
        _dbContext.SaveChanges();

        var driverProfile = new DriverProfile {
            Id = Guid.NewGuid(),
            UserId = user.Id,
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

        // Ensure we don't have this profile already
        var existing = await _dbContext.DriverProfiles.FirstOrDefaultAsync(d => d.UserId == driverProfile.UserId);
        if (existing != null) {
            _dbContext.DriverProfiles.Remove(existing);
            await _dbContext.SaveChangesAsync();
        }

        await _dbContext.DriverProfiles.AddAsync(driverProfile);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repo.GetByUserIdAsync(driverProfile.UserId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(driverProfile.LicenseNumber, result!.LicenseNumber);
        Assert.Equal(driverProfile.ServiceArea, result.ServiceArea);
        Assert.Equal(driverProfile.HomeBase, result.HomeBase);
    }

    [Fact]
    public async Task GetWithVehiclesAsync_Should_Return_DriverProfile_With_Trucks() {
        // Arrange

        var user = TestUserCloner.CloneWithNewId(MockUserTestData.DriverUser);
        user.RoleId = _dbContext.Roles.First(r => r.Name == RoleType.Driver.ToString()).Id;
        user.Onboarding = new OnboardingProgress { UserId = user.Id, StepCurrent = "Profile", Completed = false };
        _dbContext.Users.Add(user);
        _dbContext.SaveChanges();

        var driverWithVehicles = MockDriverTestData.CreateDriverWithVehicles();
        driverWithVehicles.UserId = user.Id;
        driverWithVehicles.Id = Guid.NewGuid();
        // Set all trucks' DriverProfileId to the new driver profile's Id
        foreach (var t in driverWithVehicles.Trucks) {
            t.DriverProfileId = driverWithVehicles.Id;
        }

        // Ensure we don't have this profile already
        var existing = await _dbContext.DriverProfiles.FirstOrDefaultAsync(d => d.UserId == driverWithVehicles.UserId);
        if (existing != null) {
            _dbContext.DriverProfiles.Remove(existing);
            await _dbContext.SaveChangesAsync();
        }

        await _dbContext.DriverProfiles.AddAsync(driverWithVehicles);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repo.GetWithVehiclesAsync(driverWithVehicles.UserId);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result!.Trucks);
        Assert.Equal(driverWithVehicles.Trucks.Count, result.Trucks.Count);
    }

    [Fact]
    public async Task GetVehiclesAsync_Should_Return_Driver_Trucks() {
        // Arrange

        var user = TestUserCloner.CloneWithNewId(MockUserTestData.DriverUser);
        user.RoleId = _dbContext.Roles.First(r => r.Name == RoleType.Driver.ToString()).Id;
        user.Onboarding = new OnboardingProgress { UserId = user.Id, StepCurrent = "Profile", Completed = false };
        _dbContext.Users.Add(user);
        _dbContext.SaveChanges();

        var driverWithVehicles = MockDriverTestData.CreateDriverWithVehicles();
        driverWithVehicles.UserId = user.Id;
        driverWithVehicles.Id = Guid.NewGuid();
        foreach (var t in driverWithVehicles.Trucks) {
            t.DriverProfileId = driverWithVehicles.Id;
        }

        // Ensure we don't have this profile already
        var existing = await _dbContext.DriverProfiles.FirstOrDefaultAsync(d => d.UserId == driverWithVehicles.UserId);
        if (existing != null) {
            _dbContext.DriverProfiles.Remove(existing);
            await _dbContext.SaveChangesAsync();
        }

        await _dbContext.DriverProfiles.AddAsync(driverWithVehicles);
        await _dbContext.SaveChangesAsync();

        // Act
        var trucks = await _repo.GetVehiclesAsync(driverWithVehicles.UserId);

        // Assert
        Assert.NotNull(trucks);
        Assert.Equal(driverWithVehicles.Trucks.Count, trucks.Count);
    }

    [Fact]
    public async Task GetVehicleByIdAsync_Should_Return_Truck() {
        // Arrange

        var user = TestUserCloner.CloneWithNewId(MockUserTestData.DriverUser);
        user.RoleId = _dbContext.Roles.First(r => r.Name == RoleType.Driver.ToString()).Id;
        user.Onboarding = new OnboardingProgress { UserId = user.Id, StepCurrent = "Profile", Completed = false };
        _dbContext.Users.Add(user);
        _dbContext.SaveChanges();

        var driverWithVehicles = MockDriverTestData.CreateDriverWithVehicles();
        driverWithVehicles.UserId = user.Id;
        driverWithVehicles.Id = Guid.NewGuid();
        foreach (var t in driverWithVehicles.Trucks) {
            t.DriverProfileId = driverWithVehicles.Id;
        }

        // Ensure we don't have this profile already
        var existing = await _dbContext.DriverProfiles.FirstOrDefaultAsync(d => d.UserId == driverWithVehicles.UserId);
        if (existing != null) {
            _dbContext.DriverProfiles.Remove(existing);
            await _dbContext.SaveChangesAsync();
        }

        await _dbContext.DriverProfiles.AddAsync(driverWithVehicles);
        await _dbContext.SaveChangesAsync();

        var truck = driverWithVehicles.Trucks.First();

        // Act
        var result = await _repo.GetVehicleByIdAsync(truck.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(truck.Id, result!.Id);
        Assert.Equal(truck.LicensePlate, result.LicensePlate);
    }

    [Fact]
    public async Task AddVehicleAsync_Should_Add_Truck_To_Driver() {
        // Arrange

        var user = TestUserCloner.CloneWithNewId(MockUserTestData.DriverUser);
        user.RoleId = _dbContext.Roles.First(r => r.Name == RoleType.Driver.ToString()).Id;
        user.Onboarding = new OnboardingProgress { UserId = user.Id, StepCurrent = "Profile", Completed = false };
        _dbContext.Users.Add(user);
        _dbContext.SaveChanges();

        var driverProfile = new DriverProfile {
            Id = Guid.NewGuid(),
            UserId = user.Id,
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

        // Ensure we don't have this profile already
        var existing = await _dbContext.DriverProfiles.FirstOrDefaultAsync(d => d.UserId == driverProfile.UserId);
        if (existing != null) {
            _dbContext.DriverProfiles.Remove(existing);
            await _dbContext.SaveChangesAsync();
        }

        await _dbContext.DriverProfiles.AddAsync(driverProfile);
        await _dbContext.SaveChangesAsync();


        var truck = MockTruckTestData.CreateUniqueValidTruck();
        truck.DriverProfileId = driverProfile.Id;

        // Ensure all required related entities exist in the test DB
        if (!_dbContext.TruckTypes.Any(e => e.Id == truck.TruckTypeId))
            _dbContext.TruckTypes.Add(new TruckType { Id = truck.TruckTypeId, Name = "Pickup" });
        if (!_dbContext.TruckMakes.Any(e => e.Id == truck.TruckMakeId))
            _dbContext.TruckMakes.Add(new TruckMake { Id = truck.TruckMakeId, Name = "Toyota" });
        if (!_dbContext.TruckModels.Any(e => e.Id == truck.TruckModelId))
            _dbContext.TruckModels.Add(new TruckModel { Id = truck.TruckModelId, Name = "Tundra", MakeId = truck.TruckMakeId });
        if (truck.TruckCategoryId.HasValue && !_dbContext.TruckCategories.Any(e => e.Id == truck.TruckCategoryId.Value))
            _dbContext.TruckCategories.Add(new TruckCategory { Id = truck.TruckCategoryId.Value, Name = "Small Load" });
        if (truck.BedTypeId.HasValue && !_dbContext.BedTypes.Any(e => e.Id == truck.BedTypeId.Value))
            _dbContext.BedTypes.Add(new BedType { Id = truck.BedTypeId.Value, Name = "Open Bed" });
        _dbContext.SaveChanges();

        // Assign navigation properties explicitly for testing
        truck.TruckType = _dbContext.TruckTypes.Find(truck.TruckTypeId);
        truck.TruckMake = _dbContext.TruckMakes.Find(truck.TruckMakeId);
        truck.TruckModel = _dbContext.TruckModels.Find(truck.TruckModelId);
        if (truck.TruckCategoryId.HasValue)
            truck.TruckCategory = _dbContext.TruckCategories.Find(truck.TruckCategoryId.Value);
        if (truck.BedTypeId.HasValue)
            truck.BedTypeNav = _dbContext.BedTypes.Find(truck.BedTypeId.Value);
        truck.DriverProfile = driverProfile;

        // Act
        await _repo.AddVehicleAsync(truck);
        await _dbContext.SaveChangesAsync();

        // Assert
        var addedTruck = await _dbContext.Trucks.FindAsync(truck.Id);
        Assert.NotNull(addedTruck);
        Assert.Equal(truck.LicensePlate, addedTruck!.LicensePlate);
        Assert.Equal(driverProfile.Id, addedTruck.DriverProfileId);
    }

    [Fact]
    public async Task UpdateVehicleAsync_Should_Update_Truck() {
        // Arrange

        var user = TestUserCloner.CloneWithNewId(MockUserTestData.DriverUser);
        user.RoleId = _dbContext.Roles.First(r => r.Name == RoleType.Driver.ToString()).Id;
        user.Onboarding = new OnboardingProgress { UserId = user.Id, StepCurrent = "Profile", Completed = false };
        _dbContext.Users.Add(user);
        _dbContext.SaveChanges();

        var driverWithVehicles = MockDriverTestData.CreateDriverWithVehicles();
        driverWithVehicles.UserId = user.Id;
        driverWithVehicles.Id = Guid.NewGuid();
        foreach (var vehicleTruck in driverWithVehicles.Trucks) {
            vehicleTruck.DriverProfileId = driverWithVehicles.Id;
        }

        // Ensure we don't have this profile already
        var existing = await _dbContext.DriverProfiles.FirstOrDefaultAsync(d => d.UserId == driverWithVehicles.UserId);
        if (existing != null) {
            _dbContext.DriverProfiles.Remove(existing);
            await _dbContext.SaveChangesAsync();
        }

        await _dbContext.DriverProfiles.AddAsync(driverWithVehicles);
        await _dbContext.SaveChangesAsync();

        var truck = driverWithVehicles.Trucks.First();
        const bool newActiveStatus = false;
        truck.IsActive = newActiveStatus;

        // Act
        await _repo.UpdateVehicleAsync(truck);
        await _dbContext.SaveChangesAsync();

        // Assert
        var updatedTruck = await _dbContext.Trucks.FindAsync(truck.Id);
        Assert.NotNull(updatedTruck);
        Assert.Equal(newActiveStatus, updatedTruck!.IsActive);
    }

    [Fact]
    public async Task DeleteVehicleAsync_Should_Remove_Truck() {
        // Arrange

        var user = TestUserCloner.CloneWithNewId(MockUserTestData.DriverUser);
        user.RoleId = _dbContext.Roles.First(r => r.Name == RoleType.Driver.ToString()).Id;
        user.Onboarding = new OnboardingProgress { UserId = user.Id, StepCurrent = "Profile", Completed = false };
        _dbContext.Users.Add(user);
        _dbContext.SaveChanges();

        var driverWithVehicles = MockDriverTestData.CreateDriverWithVehicles();
        driverWithVehicles.UserId = user.Id;
        driverWithVehicles.Id = Guid.NewGuid();
        foreach (var vehicleTruck in driverWithVehicles.Trucks) {
            vehicleTruck.DriverProfileId = driverWithVehicles.Id;
        }

        // Ensure we don't have this profile already
        var existing = await _dbContext.DriverProfiles.FirstOrDefaultAsync(d => d.UserId == driverWithVehicles.UserId);
        if (existing != null) {
            _dbContext.DriverProfiles.Remove(existing);
            await _dbContext.SaveChangesAsync();
        }

        await _dbContext.DriverProfiles.AddAsync(driverWithVehicles);
        await _dbContext.SaveChangesAsync();

        var truck = driverWithVehicles.Trucks.First();

        // Act
        await _repo.DeleteVehicleAsync(truck.Id);
        await _dbContext.SaveChangesAsync();

        // Assert
        var deletedTruck = await _dbContext.Trucks.FindAsync(truck.Id);
        Assert.Null(deletedTruck);
    }

    public void Dispose() {
        _dbContext?.Dispose();
        _connection?.Dispose();
    }
}
