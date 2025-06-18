using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Truckero.Core.Entities;
using Truckero.Diagnostics.Data;
using Truckero.Infrastructure.Data;
using Truckero.Infrastructure.Tests.Helpers;

namespace Truckero.Infrastructure.Tests.Integration
{
    public abstract class IntegrationTestBase : IDisposable
    {
        protected readonly IServiceProvider ServiceProvider;
        protected readonly AppDbContext DbContext;

        protected IntegrationTestBase()
        {
            var services = new ServiceCollection();
            
            // Configure DbContext with SQLite in-memory for integration tests
            services.AddDbContext<AppDbContext>(options => 
                options.UseSqlite("DataSource=:memory:"));
            
            ServiceProvider = services.BuildServiceProvider();
            
            // Create and open the database
            DbContext = ServiceProvider.GetRequiredService<AppDbContext>();
            DbContext.Database.OpenConnection();
            DbContext.Database.EnsureCreated();
            
            // Seed test data using MockUserTestData and MockTruckTestData
            SeedTestData().GetAwaiter().GetResult();
        }

        private async Task SeedTestData()
        {
            // Modified to use the ensure methods for adding entities
            
            // First add the basic reference data that doesn't have dependencies
            await SafeAddReferenceDataAsync();
            
            // Then add users (using the ensure method)
            foreach (var user in MockUserTestData.GetAllTestUsers())
            {
                await DbContext.EnsureUserExistsAsync(user);
            }
            
            // Create and link driver profiles
            await EnsureDriverProfileAsync(MockUserTestData.Ids.DriverUserId);
            
            // No need to save changes here as the EnsureXXXExists methods already save
        }

        private async Task SafeAddReferenceDataAsync()
        {
            // Add reference data, checking for existence first
            // Truck types
            foreach (var item in new[] { MockTruckTestData.PickupTruckType, MockTruckTestData.VanTruckType, MockTruckTestData.FlatbedTruckType })
            {
                if (!await DbContext.TruckTypes.AnyAsync(t => t.Id == item.Id))
                {
                    DbContext.TruckTypes.Add(item);
                }
            }
            
            // Add bed types, categories, etc. (similar pattern)

            await DbContext.SaveChangesAsync();
        }

        private async Task EnsureDriverProfileAsync(Guid driverUserId)
        {
            // Check if a driver profile already exists for this user
            if (!await DbContext.DriverProfiles.AnyAsync(dp => dp.UserId == driverUserId))
            {
                var driverProfile = new DriverProfile
                {
                    Id = Guid.NewGuid(),
                    UserId = driverUserId,
                    LicenseNumber = "D12345678",
                    LicenseExpiry = DateTime.Now.AddYears(2)
                };
                
                DbContext.DriverProfiles.Add(driverProfile);
                await DbContext.SaveChangesAsync();
            }
        }

        public void Dispose()
        {
            DbContext.Database.CloseConnection();
            DbContext.Dispose();
        }
    }
}
