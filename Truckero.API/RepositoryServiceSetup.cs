using Truckero.Core.Interfaces;
using Truckero.Core.Interfaces.Repositories;
using Truckero.Infrastructure.Repositories;

namespace Truckero.API;

public static class RepositoryServiceExtensions
{
    public static IServiceCollection AddAuthTokenRepository(this IServiceCollection services)
    {
        //#if DEBUG || UNITTESTING
        //        // 🔁 1. Create shared in-memory DbContextOptions
        //        var sharedDbOptions = new DbContextOptionsBuilder<AppDbContext>()
        //            .UseInMemoryDatabase("AuthToken_TestDb")
        //            .Options;

        //        // ✅ 2. Register the shared options as singleton
        //        services.AddSingleton(sharedDbOptions);

        //        // ✅ 3. Register AppDbContext using the shared options
        //        services.AddScoped<AppDbContext>(provider =>
        //        {
        //            var options = provider.GetRequiredService<DbContextOptions<AppDbContext>>();
        //            return new AppDbContext(options);
        //        });

        //        // ✅ 4. Inject the repository using the scoped context
        //        services.AddScoped<IAuthTokenRepository, AuthTokenRepository>();
        //#else
        services.AddScoped<IAuthTokenRepository, AuthTokenRepository>();
//#endif

        return services;
    }

    public static IServiceCollection AddRoleRepository(this IServiceCollection services)
    {
        //#if DEBUG || UNITTESTING
        //        services.AddSingleton<IRoleRepository>(provider =>
        //        {
        //            var _ = InMemoryRepositorySetup.CreateRoleRepository(out var _);
        //            return _;
        //        });
        //#else
        services.AddScoped<IRoleRepository, RoleRepository>();
//#endif
        return services;
    }

    public static IServiceCollection AddUserRepository(this IServiceCollection services)
    {
        //#if DEBUG || UNITTESTING
        //        services.AddSingleton<IUserRepository>(provider =>
        //        {
        //            var repo = InMemoryRepositorySetup.CreateUserRepository(out var _);
        //            return repo;
        //        });
        //#else
        services.AddScoped<IUserRepository, UserRepository>();
//#endif
        return services;
    }
}
