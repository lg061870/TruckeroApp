using Microsoft.EntityFrameworkCore;

namespace Truckero.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(AppDbContext context)
    {
        var provider = context.Database.ProviderName;

        // Only run migrations if using a relational provider
        if (provider != "Microsoft.EntityFrameworkCore.InMemory")
        {
            await context.Database.MigrateAsync();
        }

        // Optionally seed data here...
    }
}
