using Microsoft.EntityFrameworkCore;
using Truckero.Core.Entities;
using Truckero.Diagnostics.Mocks;
using Truckero.Infrastructure.Data;
using Truckero.Infrastructure.Repositories;
using Xunit;

namespace Truckero.Infrastructure.Tests.Repositories;

public class VehicleRepositoryTests
{
    private readonly AppDbContext _dbContext;
    private readonly VehicleRepository _repo;
    private readonly AuditLogMockRepository _fakeAudit;

    public VehicleRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _dbContext = new AppDbContext(options);
        _fakeAudit = new AuditLogMockRepository();
        _repo = new VehicleRepository(_dbContext, _fakeAudit);

        // Seed vehicle type
        var type = new VehicleType
        {
            Id = Guid.NewGuid(),
            Name = "Van"
        };
        _dbContext.VehicleTypes.Add(type);
        _dbContext.SaveChanges();

        SeededVehicleTypeId = type.Id;
    }

    public Guid SeededVehicleTypeId { get; }

    [Fact]
    public async Task AddAsync_Should_Add_Vehicle_When_Valid()
    {
        var vehicle = new Vehicle
        {
            Id = Guid.NewGuid(),
            DriverProfileId = Guid.NewGuid(),
            LicensePlate = "ABC123",
            VehicleTypeId = SeededVehicleTypeId
        };

        await _repo.AddAsync(vehicle);

        var result = await _dbContext.Vehicles.FindAsync(vehicle.Id);
        Assert.NotNull(result);
        Assert.Equal("ABC123", result!.LicensePlate);
    }

    [Fact]
    public async Task AddAsync_Should_Throw_When_LicensePlate_Duplicate()
    {
        var driverId = Guid.NewGuid();

        var vehicle1 = new Vehicle
        {
            Id = Guid.NewGuid(),
            DriverProfileId = driverId,
            LicensePlate = "DUPLICATE",
            VehicleTypeId = SeededVehicleTypeId
        };

        var vehicle2 = new Vehicle
        {
            Id = Guid.NewGuid(),
            DriverProfileId = driverId,
            LicensePlate = "duplicate", // Case-insensitive duplicate
            VehicleTypeId = SeededVehicleTypeId
        };

        await _repo.AddAsync(vehicle1);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _repo.AddAsync(vehicle2));
        Assert.Equal("This license plate already exists for the current driver.", ex.Message);
    }

    [Fact]
    public async Task AddAsync_Should_Throw_When_VehicleType_Is_Invalid()
    {
        var vehicle = new Vehicle
        {
            Id = Guid.NewGuid(),
            DriverProfileId = Guid.NewGuid(),
            LicensePlate = "NEWPLATE",
            VehicleTypeId = Guid.NewGuid() // Invalid type
        };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _repo.AddAsync(vehicle));
        Assert.Equal("Invalid VehicleTypeId specified.", ex.Message);
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_Vehicle()
    {
        var vehicle = new Vehicle
        {
            Id = Guid.NewGuid(),
            DriverProfileId = Guid.NewGuid(),
            LicensePlate = "XYZ123",
            VehicleTypeId = SeededVehicleTypeId
        };

        await _dbContext.Vehicles.AddAsync(vehicle);
        await _dbContext.SaveChangesAsync();

        var result = await _repo.GetByIdAsync(vehicle.Id);
        Assert.NotNull(result);
        Assert.Equal("XYZ123", result!.LicensePlate);
    }

    [Fact]
    public async Task GetByDriverIdAsync_Should_Return_Vehicles()
    {
        var driverId = Guid.NewGuid();

        var vehicle1 = new Vehicle
        {
            Id = Guid.NewGuid(),
            DriverProfileId = driverId,
            LicensePlate = "AAA111",
            VehicleTypeId = SeededVehicleTypeId
        };
        var vehicle2 = new Vehicle
        {
            Id = Guid.NewGuid(),
            DriverProfileId = driverId,
            LicensePlate = "BBB222",
            VehicleTypeId = SeededVehicleTypeId
        };

        await _dbContext.Vehicles.AddRangeAsync(vehicle1, vehicle2);
        await _dbContext.SaveChangesAsync();

        var result = await _repo.GetByDriverIdAsync(driverId);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task DeleteAsync_Should_Remove_Vehicle()
    {
        var vehicle = new Vehicle
        {
            Id = Guid.NewGuid(),
            DriverProfileId = Guid.NewGuid(),
            LicensePlate = "DELETE",
            VehicleTypeId = SeededVehicleTypeId
        };

        await _dbContext.Vehicles.AddAsync(vehicle);
        await _dbContext.SaveChangesAsync();

        await _repo.DeleteAsync(vehicle.Id);

        var deleted = await _dbContext.Vehicles.FindAsync(vehicle.Id);
        Assert.Null(deleted);
    }

    [Fact]
    public async Task UpdateAsync_Should_Modify_Existing_Vehicle()
    {
        var vehicle = new Vehicle
        {
            Id = Guid.NewGuid(),
            DriverProfileId = Guid.NewGuid(),
            LicensePlate = "ORIGINAL",
            VehicleTypeId = SeededVehicleTypeId
        };

        await _dbContext.Vehicles.AddAsync(vehicle);
        await _dbContext.SaveChangesAsync();

        vehicle.LicensePlate = "UPDATED";
        await _repo.UpdateAsync(vehicle);

        var updated = await _dbContext.Vehicles.FindAsync(vehicle.Id);
        Assert.Equal("UPDATED", updated!.LicensePlate);
    }
}
