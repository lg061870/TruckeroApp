using Microsoft.EntityFrameworkCore;
using Truckero.Core.Entities;
using Truckero.Core.Interfaces;
using Truckero.Core.Interfaces.Repositories;
using Truckero.Infrastructure.Data;
using Truckero.Infrastructure.Repositories;
using Truckero.Infrastructure.Services;

namespace Truckero.Diagnostics.InMemoryRepositories;

public static class InMemoryRepositorySetup
{
    public static IAuthTokenRepository CreateAuthTokenRepository(out AppDbContext context)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"AuthTokens_{Guid.NewGuid()}")
            .Options;

        context = new AppDbContext(options);

        // Optionally seed a token here if needed
        // context.AuthTokens.Add(new AuthToken { ... });
        // context.SaveChanges();

        return new AuthTokenRepository(context);
    }

    public static IRoleRepository CreateRoleRepository(out AppDbContext context)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: $"Roles_{Guid.NewGuid()}")
            .Options;

        context = new AppDbContext(options);

        // 🚨 Ensure seeding is triggered
        context.Database.EnsureCreated();

        return new RoleRepository(context);
    }

    public static IUserRepository CreateUserRepository(out AppDbContext context)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"Users_{Guid.NewGuid()}")
            .Options;

        context = new AppDbContext(options);

        // 🚨 Use real hashed password in production
        var passwordHash = new HashService().Hash("pass123");

        context.Users.Add(new User
        {
            Id = Guid.Parse("18b1b874-bab5-449d-8ff0-251758e9621b"),
            Email = "test@truckero.app",
            PasswordHash = passwordHash,
            PhoneNumber = "123-456-7890",
            RoleId = Guid.Parse("00000000-0000-0000-0000-000000000002"), // Customer
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            EmailVerified = true,
            Onboarding = new OnboardingProgress() // required non-null nav prop
        });

        context.SaveChanges();

        return new UserRepository(context);
    }


}
