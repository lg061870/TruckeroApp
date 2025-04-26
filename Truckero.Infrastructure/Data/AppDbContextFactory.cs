using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Truckero.Infrastructure.Data;

namespace Truckero.Infrastructure.Data
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

            // ⚡ Specify your connection string here manually
            optionsBuilder.UseSqlServer(
                "Server=localhost\\SQLEXPRESS;Database=TruckeroDev;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;"
            );

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}

