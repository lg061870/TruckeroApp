using Microsoft.EntityFrameworkCore;
using System.IO;

namespace Truckero.Diagnostics
{
    /// <summary>
    /// Provides DbContext instances for tests using the cloned SQLite database.
    /// </summary>
    public static class TestDbContextProvider
    {
        private static readonly DatabaseCloneManager _cloneManager = new DatabaseCloneManager(
            Path.Combine(Directory.GetCurrentDirectory(), "..\\..\\..\\..\\Truckero.Diagnostics\\TestTemplate.sqlite")
        );
        
        /// <summary>
        /// Creates a DbContext for testing using the current database clone.
        /// </summary>
        public static TContext CreateDbContext<TContext>() where TContext : DbContext
        {
            string connectionString = $"Data Source={_cloneManager.GetCurrentClonePath()}";
            
            var optionsBuilder = new DbContextOptionsBuilder<TContext>();
            optionsBuilder.UseSqlite(connectionString);
            
            return (TContext)Activator.CreateInstance(typeof(TContext), optionsBuilder.Options);
        }
    }
}