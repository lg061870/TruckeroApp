using Microsoft.EntityFrameworkCore;
using Truckero.Infrastructure.Data;

public static class TestTemplateBuilder
{
    private static readonly string DbDir = Path.Combine("..", "..", "..", "..", "Truckero.Diagnostics", "CloneDBs");
    private static readonly string DbPath = Path.Combine(DbDir, "TestTemplate.sqlite");
    private static readonly string TimestampPath = Path.Combine(DbDir, "TestTemplate.timestamp");

    public static string EnsureTestDatabase()
    {
        if (!Directory.Exists(DbDir))
            Directory.CreateDirectory(DbDir);

        bool shouldCreate = true;
        if (File.Exists(TimestampPath))
        {
            var lastCreated = File.GetLastWriteTimeUtc(TimestampPath);
            shouldCreate = (DateTime.UtcNow - lastCreated) > TimeSpan.FromMinutes(3);
        }

        if (shouldCreate)
        {
            if (File.Exists(DbPath))
                File.Delete(DbPath);

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite($"DataSource={DbPath}")
                .Options;

            using var context = new AppDbContext(options);
            context.Database.EnsureCreated();
            context.SaveChanges();

            File.WriteAllText(TimestampPath, DateTime.UtcNow.ToString("O"));
        }

        return $"DataSource={DbPath}";
    }
}