using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Truckero.Core.Entities;
using Truckero.Diagnostics.Data;
using Truckero.Infrastructure.Repositories;
using Truckero.Infrastructure.Tests.Fixtures;
using Truckero.Infrastructure.Tests.Helpers;
using Xunit;

namespace Truckero.Infrastructure.Tests.Repositories
{
    public class TruckRepositoryTests : IClassFixture<TestDbContextFixture>
    {
        private readonly TestDbContextFixture _fixture;
        private readonly TruckRepository _repository;
        private readonly Mock<ILogger<TruckRepository>> _loggerMock;

        public TruckRepositoryTests(TestDbContextFixture fixture)
        {
            _fixture = fixture;
            _loggerMock = new Mock<ILogger<TruckRepository>>();
            _repository = new TruckRepository(fixture.DbContext, _loggerMock.Object);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllTrucks()
        {
            // Arrange: create a unique user and driver profile
            var user = TestUserCloner.CloneWithNewId(MockUserTestData.DriverUser);
            await _fixture.DbContext.EnsureUserExistsAsync(user);
            var driverProfile = new DriverProfile {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                LicenseNumber = "D-ALL-TRUCKS",
                LicenseExpiry = DateTime.UtcNow.AddYears(2),
                LicenseFrontUrl = "front.jpg",
                LicenseBackUrl = "back.jpg",
                ServiceArea = "TestArea",
                HomeBase = "TestBase",
                ServiceRadiusKm = 10,
                Latitude = 0,
                Longitude = 0
            };
            await _fixture.DbContext.DriverProfiles.AddAsync(driverProfile);
            await _fixture.DbContext.SaveChangesAsync();
            // Add a truck for this driver
            var truck = MockTruckTestData.CreateUniqueValidTruck();
            truck.DriverProfileId = driverProfile.Id;
            await _repository.AddAsync(truck);
            await _fixture.DbContext.SaveChangesAsync();

            // Act
            var trucks = await _repository.GetAllAsync();

            // Assert
            Assert.NotNull(trucks);
            Assert.NotEmpty(trucks);
            Assert.Contains(trucks, t => t.Id == truck.Id);
        }

        [Fact]
        public async Task GetByIdAsync_WithValidId_ReturnsTruck()
        {
            // Arrange
            // Arrange: create a unique user and driver profile
            var user = TestUserCloner.CloneWithNewId(MockUserTestData.DriverUser);
            await _fixture.DbContext.EnsureUserExistsAsync(user);
            var driverProfile = new DriverProfile {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                LicenseNumber = "D-GETBYID",
                LicenseExpiry = DateTime.UtcNow.AddYears(2),
                LicenseFrontUrl = "front.jpg",
                LicenseBackUrl = "back.jpg",
                ServiceArea = "TestArea",
                HomeBase = "TestBase",
                ServiceRadiusKm = 10,
                Latitude = 0,
                Longitude = 0
            };
            await _fixture.DbContext.DriverProfiles.AddAsync(driverProfile);
            await _fixture.DbContext.SaveChangesAsync();
            var mockTruck = MockTruckTestData.CreateUniqueValidTruck();
            mockTruck.DriverProfileId = driverProfile.Id;
            await _fixture.DbContext.EnsureTruckExistsAsync(mockTruck);

            // Act
            var truck = await _repository.GetByIdAsync(mockTruck.Id);

            // Assert
            Assert.NotNull(truck);
            Assert.Equal(mockTruck.Id, truck.Id);
            Assert.Equal(mockTruck.LicensePlate, truck.LicensePlate);
        }

        [Fact]
        public async Task GetByIdAsync_WithInvalidId_ReturnsNull()
        {
            // Arrange
            var invalidTruckId = Guid.NewGuid();

            // Act
            var truck = await _repository.GetByIdAsync(invalidTruckId);

            // Assert
            Assert.Null(truck);
        }

        [Fact]
        public async Task AddAsync_AddsNewTruck()
        {
            // Arrange: create a unique user and driver profile
            var user = TestUserCloner.CloneWithNewId(MockUserTestData.DriverUser);
            await _fixture.DbContext.EnsureUserExistsAsync(user);
            var driverProfile = new DriverProfile {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                LicenseNumber = "D-ADD",
                LicenseExpiry = DateTime.UtcNow.AddYears(2),
                LicenseFrontUrl = "front.jpg",
                LicenseBackUrl = "back.jpg",
                ServiceArea = "TestArea",
                HomeBase = "TestBase",
                ServiceRadiusKm = 10,
                Latitude = 0,
                Longitude = 0
            };
            await _fixture.DbContext.DriverProfiles.AddAsync(driverProfile);
            await _fixture.DbContext.SaveChangesAsync();
            var newTruck = MockTruckTestData.CreateUniqueValidTruck();
            newTruck.DriverProfileId = driverProfile.Id;

            // Act
            await _repository.AddAsync(newTruck);
            await _fixture.DbContext.SaveChangesAsync();
            var addedTruck = await _repository.GetByIdAsync(newTruck.Id);

            // Assert
            Assert.NotNull(addedTruck);
            Assert.Equal(newTruck.LicensePlate, addedTruck.LicensePlate);
            Assert.Equal(newTruck.Year, addedTruck.Year);
        }

        [Fact]
        public async Task UpdateAsync_UpdatesExistingTruck()
        {
            // Arrange: create a unique user and driver profile
            var user = TestUserCloner.CloneWithNewId(MockUserTestData.DriverUser);
            await _fixture.DbContext.EnsureUserExistsAsync(user);
            var driverProfile = new DriverProfile {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                LicenseNumber = "D-UPDATE",
                LicenseExpiry = DateTime.UtcNow.AddYears(2),
                LicenseFrontUrl = "front.jpg",
                LicenseBackUrl = "back.jpg",
                ServiceArea = "TestArea",
                HomeBase = "TestBase",
                ServiceRadiusKm = 10,
                Latitude = 0,
                Longitude = 0
            };
            await _fixture.DbContext.DriverProfiles.AddAsync(driverProfile);
            await _fixture.DbContext.SaveChangesAsync();
            var truck = MockTruckTestData.CreateUniqueValidTruck();
            truck.DriverProfileId = driverProfile.Id;
            await _repository.AddAsync(truck);
            await _fixture.DbContext.SaveChangesAsync();
            // Modify the truck
            const bool newIsActiveStatus = false;
            truck.IsActive = newIsActiveStatus;

            // Act
            await _repository.UpdateAsync(truck);
            await _fixture.DbContext.SaveChangesAsync();
            // Retrieve the updated truck
            var updatedTruck = await _repository.GetByIdAsync(truck.Id);

            // Assert
            Assert.NotNull(updatedTruck);
            Assert.Equal(newIsActiveStatus, updatedTruck.IsActive);
        }

        [Fact]
        public async Task DeleteAsync_RemovesTruck()
        {
            // Arrange: create a unique user and driver profile
            var user = TestUserCloner.CloneWithNewId(MockUserTestData.DriverUser);
            await _fixture.DbContext.EnsureUserExistsAsync(user);
            var driverProfile = new DriverProfile {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                LicenseNumber = "D-DELETE",
                LicenseExpiry = DateTime.UtcNow.AddYears(2),
                LicenseFrontUrl = "front.jpg",
                LicenseBackUrl = "back.jpg",
                ServiceArea = "TestArea",
                HomeBase = "TestBase",
                ServiceRadiusKm = 10,
                Latitude = 0,
                Longitude = 0
            };
            await _fixture.DbContext.DriverProfiles.AddAsync(driverProfile);
            await _fixture.DbContext.SaveChangesAsync();
            var truck = MockTruckTestData.CreateUniqueValidTruck();
            truck.DriverProfileId = driverProfile.Id;
            await _repository.AddAsync(truck);
            await _fixture.DbContext.SaveChangesAsync();

            // Act
            await _repository.DeleteAsync(truck.Id);
            await _fixture.DbContext.SaveChangesAsync();
            // Attempt to retrieve the deleted truck
            var deletedTruck = await _repository.GetByIdAsync(truck.Id);

            // Assert
            Assert.Null(deletedTruck);
        }

        [Fact]
        public async Task GetByDriverIdAsync_WithValidDriverId_ReturnsTrucks()
        {
            // Arrange: create a unique user and driver profile
            var user = TestUserCloner.CloneWithNewId(MockUserTestData.DriverUser);
            await _fixture.DbContext.EnsureUserExistsAsync(user);
            var driverProfile = new DriverProfile {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                LicenseNumber = "D-GETBYDRIVERID",
                LicenseExpiry = DateTime.UtcNow.AddYears(2),
                LicenseFrontUrl = "front.jpg",
                LicenseBackUrl = "back.jpg",
                ServiceArea = "TestArea",
                HomeBase = "TestBase",
                ServiceRadiusKm = 10,
                Latitude = 0,
                Longitude = 0
            };
            await _fixture.DbContext.DriverProfiles.AddAsync(driverProfile);
            await _fixture.DbContext.SaveChangesAsync();
            var truck1 = MockTruckTestData.CreateUniqueValidTruck();
            var truck2 = MockTruckTestData.CreateUniqueValidTruck();
            truck1.DriverProfileId = driverProfile.Id;
            truck2.DriverProfileId = driverProfile.Id;
            await _repository.AddAsync(truck1);
            await _repository.AddAsync(truck2);
            await _fixture.DbContext.SaveChangesAsync();

            // Act
            var trucks = await _repository.GetByDriverIdAsync(driverProfile.Id);

            // Assert
            Assert.NotNull(trucks);
            Assert.Contains(trucks, t => t.Id == truck1.Id);
            Assert.Contains(trucks, t => t.Id == truck2.Id);
            Assert.All(trucks, t => Assert.Equal(driverProfile.Id, t.DriverProfileId));
        }

        [Fact]
        public async Task GetAvailableTrucksAsync_ReturnsAvailableTrucks()
        {
            // Arrange: create a unique user and driver profile
            var user = TestUserCloner.CloneWithNewId(MockUserTestData.DriverUser);
            await _fixture.DbContext.EnsureUserExistsAsync(user);
            var driverProfile = new DriverProfile {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                LicenseNumber = "D-AVAILABLE",
                LicenseExpiry = DateTime.UtcNow.AddYears(2),
                LicenseFrontUrl = "front.jpg",
                LicenseBackUrl = "back.jpg",
                ServiceArea = "TestArea",
                HomeBase = "TestBase",
                ServiceRadiusKm = 10,
                Latitude = 0,
                Longitude = 0
            };
            await _fixture.DbContext.DriverProfiles.AddAsync(driverProfile);
            await _fixture.DbContext.SaveChangesAsync();
            var activeTruck = MockTruckTestData.CreateUniqueValidTruck();
            activeTruck.IsActive = true;
            activeTruck.DriverProfileId = driverProfile.Id;
            var inactiveTruck = MockTruckTestData.CreateUniqueValidTruck();
            inactiveTruck.IsActive = false;
            inactiveTruck.DriverProfileId = driverProfile.Id;
            await _repository.AddAsync(activeTruck);
            await _repository.AddAsync(inactiveTruck);
            await _fixture.DbContext.SaveChangesAsync();

            // Act
            var trucks = await _repository.GetAvailableTrucksAsync();

            // Assert
            Assert.NotNull(trucks);
            Assert.Contains(trucks, t => t.Id == activeTruck.Id);
            Assert.DoesNotContain(trucks, t => t.Id == inactiveTruck.Id);
            Assert.All(trucks, t => Assert.True(t.IsActive));
        }
    }
}
