using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Truckero.Core.Entities;
using Truckero.Diagnostics.Data;
using Truckero.Infrastructure.Data;

namespace Truckero.Infrastructure.Tests.Fixtures
{
    public class TestDbContextFixture : IDisposable
    {
        public AppDbContext DbContext { get; private set; }

        public TestDbContextFixture()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            DbContext = new AppDbContext(options);
            DbContext.Database.EnsureCreated();
            SeedDatabase();
        }

        private void SeedDatabase()
        {
            // Make sure roles are seeded
            SeedRoles();
            
            // Seed truck data and other reference data
            SeedTruckData();
            
            DbContext.SaveChanges();
        }
        
        private void SeedRoles()
        {
            // Add all roles from MockRoleTestData
            var roles = MockRoleTestData.GetAllRoles();
            foreach (var role in roles)
            {
                var existingRole = DbContext.Roles.FirstOrDefault(r => r.Id == role.Id);
                if (existingRole == null)
                {
                    DbContext.Roles.Add(role);
                }
            }
        }
        
        private void SeedTruckData()
        {
            // Add mock truck types, makes, models, etc.
            // This is your existing code for seeding truck data
        }

        public void Dispose()
        {
            DbContext.Database.EnsureDeleted();
            DbContext.Dispose();
        }
    }
}
