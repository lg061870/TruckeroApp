using Microsoft.EntityFrameworkCore;
using Truckero.Core.Entities;
using Truckero.Core.Enums;
using Truckero.Diagnostics.Seeders;
using Truckero.Infrastructure.Data;
using Truckero.Infrastructure.Repositories;

namespace Truckero.Infrastructure.Tests.Repositories;

public class DriverRepositoryTests
{
    private readonly AppDbContext _dbContext;
    private readonly DriverRepository _repo;

    public DriverRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _dbContext = new AppDbContext(options);
        _repo = new DriverRepository(_dbContext);

        // Seed role
        var driverRole = TestSeedHelpers.SeededRole(Core.Enums.RoleType.Driver);
        _dbContext.Roles.Add(driverRole);

        // Seed user
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "driver@example.com",
            RoleId = driverRole.Id,
            Role = driverRole,
            Onboarding = new OnboardingProgress { UserId = Guid.NewGuid() }
        };
        _dbContext.Users.Add(user);
        _dbContext.SaveChanges();

        SeededUser = user;
    }

    public User SeededUser { get; }

    [Fact]
    public async Task AddAsync_And_Save_Should_Persist_DriverProfile()
    {
        var driver = new DriverProfile
        {
            Id = Guid.NewGuid(),
            UserId = SeededUser.Id,
            LicenseNumber = "TX1234567",
            LicenseExpiry = DateTime.UtcNow.AddYears(3)
        };

        await _repo.AddAsync(driver);
        await _repo.SaveChangesAsync();

        var found = await _dbContext.DriverProfiles.FirstOrDefaultAsync(d => d.UserId == driver.UserId);
        Assert.NotNull(found);
        Assert.Equal("TX1234567", found!.LicenseNumber);
    }

    [Fact]
    public async Task GetByUserIdAsync_Should_Return_DriverProfile()
    {
        var driver = new DriverProfile
        {
            Id = Guid.NewGuid(),
            UserId = SeededUser.Id,
            LicenseNumber = "FL9876543",
            LicenseExpiry = DateTime.UtcNow.AddYears(2)
        };

        await _dbContext.DriverProfiles.AddAsync(driver);
        await _dbContext.SaveChangesAsync();

        var result = await _repo.GetByUserIdAsync(driver.UserId);
        Assert.NotNull(result);
        Assert.Equal("FL9876543", result!.LicenseNumber);
    }

    [Fact]
    public async Task GetByUserIdAsync_Should_Return_Null_If_NotFound()
    {
        var result = await _repo.GetByUserIdAsync(Guid.NewGuid());
        Assert.Null(result);
    }
}
