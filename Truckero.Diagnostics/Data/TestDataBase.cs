using System;
using System.IO;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Truckero.Infrastructure.Data;

namespace Truckero.Diagnostics.Data
{
    /// <summary>
    /// Base class for test data and test helpers
    /// </summary>
    public static class TestDataBase
    {
        /// <summary>
        /// Gets the path to the test template SQLite database
        /// </summary>
        public static string GetTestDbPath()
        {
            return Path.Combine("..", "..", "..", "..", "Truckero.Diagnostics", "CloneDBs", "TestTemplate.sqlite");
        }

        /// <summary>
        /// Creates a new DbContext using the test template SQLite database
        /// </summary>
        public static AppDbContext CreateDbContext()
        {
            var dbPath = GetTestDbPath();
            
            // Ensure the file exists
            if (!File.Exists(dbPath))
            {
                throw new FileNotFoundException($"The SQLite database file was not found at: {dbPath}. Please run TestTemplateBuilder first to create it.");
            }

            var connectionString = $"DataSource={dbPath}";
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(connectionString)
                .Options;

            return new AppDbContext(options);
        }

        /// <summary>
        /// Creates a new in-memory DbContext for isolated tests
        /// </summary>
        public static (AppDbContext dbContext, SqliteConnection connection) CreateInMemoryDbContext()
        {
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();
            
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(connection)
                .Options;

            var context = new AppDbContext(options);
            context.Database.EnsureCreated();
            
            // Seed basic data
            SeedBasicTypes(context);
            
            return (context, connection);
        }

        /// <summary>
        /// Seeds basic necessary types like TruckType for testing
        /// </summary>
        private static void SeedBasicTypes(AppDbContext context)
        {
            // Add truck types if none exist
            if (!context.TruckTypes.Any())
            {
                context.TruckTypes.Add(MockTruckTestData.PickupTruckType);
                context.TruckTypes.Add(MockTruckTestData.VanTruckType);
                context.TruckTypes.Add(MockTruckTestData.FlatbedTruckType);
                context.SaveChanges();
            }
            
            // Add bed types if none exist
            if (!context.BedTypes.Any())
            {
                context.BedTypes.Add(MockTruckTestData.StandardBed);
                context.BedTypes.Add(MockTruckTestData.FlatBed);
                context.SaveChanges();
            }
        }
    }
}