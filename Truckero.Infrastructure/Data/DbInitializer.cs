using Microsoft.EntityFrameworkCore;

namespace Truckero.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(AppDbContext context)
    {
        await context.Database.MigrateAsync();

        // Optional: Add tenant-specific or dynamic bootstrap logic here in the future
    }
}
