using Microsoft.EntityFrameworkCore;
using Truckero.Infrastructure.Data;

class Program
{
    static void Main()
    {
        //// Only run if in unit testing environment
        //if (Environment.GetEnvironmentVariable("UnitTesting") != "true")
        //    return;

        var dbPath = Path.Combine(@"..\..\..\..", "Truckero.Diagnostics", "CloneDBs", "TestTemplate.sqlite");
        var dir = Path.GetDirectoryName(dbPath);

        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        bool shouldCreate = true;
        if (File.Exists(dbPath))
        {
            var lastCreated = File.GetCreationTimeUtc(dbPath);
            shouldCreate = (DateTime.UtcNow - lastCreated) > TimeSpan.FromMinutes(3);

            if (shouldCreate)
            {
                // Format timestamp for filename (e.g., 20240612T153000Z)
                var timestamp = lastCreated.ToString("yyyyMMddTHHmmssZ");
                var archivedPath = Path.Combine(dir, $"TestTemplate_{timestamp}.sqlite");
                File.Move(dbPath, archivedPath);
            }
        }

        if (shouldCreate)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite($"DataSource={dbPath}")
                .Options;

            using var context = new AppDbContext(options);
            context.Database.EnsureCreated();
            context.SaveChanges();

            Console.WriteLine("Database created and seeded.");
        }
        else
        {
            Console.WriteLine("Database creation throttled (less than 3 minutes since last creation).");
        }

        // Output the connection string for test consumption
        Console.WriteLine($"CONNECTION_STRING=DataSource={dbPath}");
    }
}
