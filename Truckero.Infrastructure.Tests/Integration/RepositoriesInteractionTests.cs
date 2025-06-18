using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Linq;
using System.Threading.Tasks;
using Truckero.Core.Entities;
using Truckero.Diagnostics.Data;
using Truckero.Infrastructure.Repositories;
using Truckero.Infrastructure.Tests.Fixtures;
using Truckero.Infrastructure.Tests.Helpers;
using Xunit;

namespace Truckero.Infrastructure.Tests.Integration
{
    public class RepositoriesInteractionTests : IClassFixture<TestDbContextFixture>
    {
        private readonly UserRepository _userRepository;
        private readonly TruckRepository _truckRepository;
        private readonly Mock<ILogger<TruckRepository>> _loggerMock;
        private readonly TestDbContextFixture _fixture;

        public RepositoriesInteractionTests(TestDbContextFixture fixture)
        {
            _fixture = fixture;
            _userRepository = new UserRepository(fixture.DbContext);
            _loggerMock = new Mock<ILogger<TruckRepository>>();
            _truckRepository = new TruckRepository(fixture.DbContext, _loggerMock.Object);
        }

        [Fact]
        public async Task CreateUserWithTruck_AllPersisted()
        {
            // Arrange - Use mock data directly from MockUserTestData and MockTruckTestData
            var user = MockUserTestData.DriverUser;
            var truck = MockTruckTestData.ToyotaTundra;
            
            // Associate the truck with the driver user (normally would be a DriverProfile ID)
            truck.DriverProfileId = user.Id;
            
            // Ensure entities don't already exist to avoid duplicates
            await _fixture.DbContext.EnsureUserExistsAsync(user);
            
            // Create a new truck with unique ID to avoid conflicts
            var uniqueTruck = MockTruckTestData.CreateUniqueValidTruck();
            uniqueTruck.DriverProfileId = user.Id;
            
            // Act
            using (var transaction = await _fixture.DbContext.Database.BeginTransactionAsync())
            {
                await _truckRepository.AddAsync(uniqueTruck);
                await _fixture.DbContext.SaveChangesAsync();
                
                await transaction.CommitAsync();
            }
            
            // Assert
            var savedUser = await _userRepository.GetUserByIdAsync(user.Id);
            var savedTruck = await _truckRepository.GetByIdAsync(uniqueTruck.Id);
            
            Assert.NotNull(savedUser);
            Assert.NotNull(savedTruck);
            
            Assert.Equal(user.Email, savedUser.Email);
            Assert.Equal(uniqueTruck.LicensePlate, savedTruck.LicensePlate);
            
            Assert.Equal(user.Id, savedTruck.DriverProfileId);
        }

        [Fact]
        public async Task DeleteDriverWithTrucks_CascadeDeletes()
        {
            // Arrange - Add a driver user and truck for this test
            var driverUser = MockUserTestData.DriverUser;
            var truck = MockTruckTestData.CreateUniqueValidTruck();
            truck.DriverProfileId = driverUser.Id;
            
            // Set up the test data
            await _userRepository.AddUserAsync(driverUser);
            await _userRepository.SaveUserChangesAsync();
            await _truckRepository.AddAsync(truck);
            await _fixture.DbContext.SaveChangesAsync();
            
            // Verify setup
            var driverTrucks = await _truckRepository.GetByDriverIdAsync(driverUser.Id);
            Assert.NotEmpty(driverTrucks);

            // Act - Delete driver which should cascade to trucks
            using (var transaction = await _fixture.DbContext.Database.BeginTransactionAsync())
            {
                await _userRepository.DeleteUserAsync(driverUser);
                await _userRepository.SaveUserChangesAsync();
                await transaction.CommitAsync();
            }

            // Assert
            var deletedUser = await _userRepository.GetUserByIdAsync(driverUser.Id);
            Assert.Null(deletedUser);
            
            // Check if trucks were deleted via cascading constraints
            foreach (var t in driverTrucks)
            {
                var deletedTruck = await _truckRepository.GetByIdAsync(t.Id);
                Assert.Null(deletedTruck);
            }
        }
        
        [Fact]
        public async Task UpdateTruckStatus_UpdatesTruck()
        {
            // Arrange - Create a test truck we can safely modify
            var truck = MockTruckTestData.CreateUniqueValidTruck();
            await _truckRepository.AddAsync(truck);
            await _fixture.DbContext.SaveChangesAsync();
            
            var originalActiveStatus = truck.IsActive;
            var newActiveStatus = !originalActiveStatus;
            
            // Act
            using (var transaction = await _fixture.DbContext.Database.BeginTransactionAsync())
            {
                var truckToUpdate = await _truckRepository.GetByIdAsync(truck.Id);
                truckToUpdate.IsActive = newActiveStatus;
                await _truckRepository.UpdateAsync(truckToUpdate);
                await _fixture.DbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            
            // Assert
            var updatedTruck = await _truckRepository.GetByIdAsync(truck.Id);
            Assert.NotNull(updatedTruck);
            Assert.Equal(newActiveStatus, updatedTruck.IsActive);
            Assert.NotEqual(originalActiveStatus, updatedTruck.IsActive);
        }
    }
}
