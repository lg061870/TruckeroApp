using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Linq;
using System.Threading.Tasks;
using Truckero.Core.Entities;
using Truckero.Diagnostics.Data;
using Truckero.Infrastructure.Repositories;
using Xunit;

namespace Truckero.Infrastructure.Tests.Integration {
    public class TruckRepositoryIntegrationTests : IntegrationTestBase {
        private readonly TruckRepository _repository;
        private readonly Mock<ILogger<TruckRepository>> _loggerMock;

        public TruckRepositoryIntegrationTests() {
            _loggerMock = new Mock<ILogger<TruckRepository>>();
            _repository = new TruckRepository(DbContext, _loggerMock.Object);
        }

        [Fact]
        public async Task TransactionCommit_SavesChanges() {
            // Arrange
            var driverProfile = await DbContext.DriverProfiles
                .FirstOrDefaultAsync(dp => dp.UserId == MockUserTestData.Ids.DriverUserId);

            Assert.NotNull(driverProfile); // Verify driver profile exists

            // Create a new truck based on mock data template
            var newTruck = new Truck {
                Id = Guid.NewGuid(),
                DriverProfileId = driverProfile.Id,
                TruckTypeId = MockTruckTestData.PickupTruckType.Id,
                TruckMakeId = MockTruckTestData.ToyotaMake.Id,
                TruckModelId = MockTruckTestData.TundraModel.Id,
                LicensePlate = "TRANS-123",
                Year = 2022,
                PhotoFrontUrl = "https://example.com/photos/transaction_test_front.jpg",
                PhotoBackUrl = "https://example.com/photos/transaction_test_back.jpg",
                TruckCategoryId = MockTruckTestData.UtilityTruckCategory.Id,
                BedTypeId = MockTruckTestData.StandardBed.Id,
                OwnershipType = OwnershipType.Owned,
                InsuranceProvider = "Test Insurance Company",
                PolicyNumber = "TIC-12345",
                IsActive = true
            };

            // Act
            using (var transaction = await DbContext.Database.BeginTransactionAsync()) {
                await _repository.AddAsync(newTruck);
                await DbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }

            // Assert
            var savedTruck = await _repository.GetByIdAsync(newTruck.Id);
            Assert.NotNull(savedTruck);
            Assert.Equal(newTruck.LicensePlate, savedTruck.LicensePlate);
            Assert.Equal(newTruck.Year, savedTruck.Year);
            Assert.Equal(newTruck.TruckMakeId, savedTruck.TruckMakeId);
            Assert.Equal(newTruck.TruckModelId, savedTruck.TruckModelId);
        }

        [Fact]
        public async Task TransactionRollback_DiscardsChanges() {
            // Arrange
            var initialCount = (await _repository.GetAllAsync()).Count();
            var driverProfile = await DbContext.DriverProfiles
                .FirstOrDefaultAsync(dp => dp.UserId == MockUserTestData.Ids.DriverUserId);

            Assert.NotNull(driverProfile); // Verify driver profile exists

            // Create a new truck based on mock data template
            var newTruck = new Truck {
                Id = Guid.NewGuid(),
                DriverProfileId = driverProfile.Id,
                TruckTypeId = MockTruckTestData.VanTruckType.Id,
                TruckMakeId = MockTruckTestData.FordMake.Id,
                TruckModelId = MockTruckTestData.TransitModel.Id,
                LicensePlate = "ROLL-123",
                Year = 2022,
                PhotoFrontUrl = "https://example.com/photos/rollback_test_front.jpg",
                PhotoBackUrl = "https://example.com/photos/rollback_test_back.jpg",
                TruckCategoryId = MockTruckTestData.CommercialTruckCategory.Id,
                OwnershipType = OwnershipType.Leased,
                InsuranceProvider = "Rollback Insurance Company",
                PolicyNumber = "RIC-67890",
                IsActive = true
            };

            // Act
            using (var transaction = await DbContext.Database.BeginTransactionAsync()) {
                await _repository.AddAsync(newTruck);
                await DbContext.SaveChangesAsync();
                // Do not commit, let it roll back
            }

            // Assert
            var afterCount = (await _repository.GetAllAsync()).Count();
            var truck = await _repository.GetByIdAsync(newTruck.Id);

            Assert.Equal(initialCount, afterCount); // Count should remain the same
            Assert.Null(truck); // Truck should not be persisted
        }

        [Fact]
        public async Task ComplexTransaction_UpdatesRelatedEntities() {
            // Arrange - Get driver profiles for reassignment
            var ownerDriverProfile = await DbContext.DriverProfiles
                .FirstOrDefaultAsync(dp => dp.UserId == MockUserTestData.Ids.DriverUserId);

            // Create a new test truck to update
            var testTruck = MockTruckTestData.CreateUniqueValidTruck();
            testTruck.DriverProfileId = ownerDriverProfile.Id;

            // Add the truck to the database
            await _repository.AddAsync(testTruck);
            await DbContext.SaveChangesAsync();

            // Create a new driver to reassign the truck to
            var newUser = new User {
                Id = Guid.NewGuid(),
                Email = "newdriver@example.com",
                FullName = "New Driver",
                PasswordHash = "HASH_VALUE_HERE",
                PhoneNumber = "555-123-4567",
                Address = "456 New Address St",
                EmailVerified = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            DbContext.Users.Add(newUser);
            await DbContext.SaveChangesAsync();

            var newDriverProfile = new DriverProfile {
                Id = Guid.NewGuid(),
                UserId = newUser.Id,
                LicenseNumber = "NEW-DL-12345",
                LicenseExpiry = DateTime.Now.AddYears(2),
                LicenseFrontUrl = "https://example.com/license/new-front.jpg",
                LicenseBackUrl = "https://example.com/license/new-back.jpg",
                HomeBase = "New City, TX",
                ServiceRadiusKm = 30,
                Latitude = 29.7604,
                Longitude = -95.3698
            };

            DbContext.DriverProfiles.Add(newDriverProfile);
            await DbContext.SaveChangesAsync();

            // Act - Change truck's driver profile and active status in a single transaction
            using (var transaction = await DbContext.Database.BeginTransactionAsync()) {
                var truckToUpdate = await _repository.GetByIdAsync(testTruck.Id);
                truckToUpdate.DriverProfileId = newDriverProfile.Id; // Reassign to new driver
                truckToUpdate.IsActive = false; // Change active status

                await _repository.UpdateAsync(truckToUpdate);
                await DbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }

            // Assert
            var updatedTruck = await _repository.GetByIdAsync(testTruck.Id);
            Assert.NotNull(updatedTruck);
            Assert.Equal(newDriverProfile.Id, updatedTruck.DriverProfileId);
            Assert.False(updatedTruck.IsActive);

            // Check new driver's trucks
            var driverTrucks = await _repository.GetByDriverIdAsync(newDriverProfile.Id);
            Assert.Contains(driverTrucks, t => t.Id == testTruck.Id);
        }
    }
}