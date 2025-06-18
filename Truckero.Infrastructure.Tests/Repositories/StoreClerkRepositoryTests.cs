using Microsoft.EntityFrameworkCore;
using Truckero.Infrastructure.Data;
using Truckero.Infrastructure.Repositories;
using Truckero.Diagnostics.Seeders;
using Truckero.Core.Enums;
using Xunit;

namespace Truckero.Infrastructure.Tests.Repositories;

public class StoreClerkRepositoryTests
{
    private readonly AppDbContext _dbContext;
    private readonly StoreClerkRepository _repo;

    public StoreClerkRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _dbContext = new AppDbContext(options);
        _repo = new StoreClerkRepository(_dbContext);

        // 🌱 Seed required role
        _dbContext.Roles.Add(TestSeedHelpers.SeededRole(RoleType.StoreClerk));
        _dbContext.SaveChanges();
    }

    [Fact]
    public async Task AddAsync_And_Save_Should_Persist_ClerkProfile()
    {
        var clerk = new StoreClerkProfile
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            CorporateEmail = "clerk@store.com",
            Verified = true
        };

        await _repo.AddAsync(clerk);
        await _repo.SaveChangesAsync();

        var found = await _dbContext.StoreClerkProfiles.FindAsync(clerk.Id);
        Assert.NotNull(found);
        Assert.Equal("clerk@store.com", found!.CorporateEmail);
    }

    [Fact]
    public async Task GetByUserIdAsync_Should_Return_Profile()
    {
        var clerk = new StoreClerkProfile
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            CorporateEmail = "lookup@store.com",
            Verified = false
        };

        await _dbContext.StoreClerkProfiles.AddAsync(clerk);
        await _dbContext.SaveChangesAsync();

        var found = await _repo.GetByUserIdAsync(clerk.UserId);
        Assert.NotNull(found);
        Assert.Equal("lookup@store.com", found!.CorporateEmail);
    }

    [Fact]
    public async Task GetByUserIdAsync_Should_Return_Null_If_NotFound()
    {
        var result = await _repo.GetByUserIdAsync(Guid.NewGuid());
        Assert.Null(result);
    }
}
