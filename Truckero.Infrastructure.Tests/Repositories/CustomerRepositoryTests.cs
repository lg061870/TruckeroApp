using Microsoft.EntityFrameworkCore;
using Truckero.Core.Entities;
using Truckero.Infrastructure.Data;
using Truckero.Infrastructure.Repositories;
using Xunit;

namespace Truckero.Infrastructure.Tests.Repositories;

public class CustomerRepositoryTests
{
    private readonly AppDbContext _dbContext;
    private readonly CustomerRepository _repo;

    public CustomerRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _dbContext = new AppDbContext(options);
        _repo = new CustomerRepository(_dbContext);
    }

    [Fact]
    public async Task AddAsync_Should_Add_Profile_To_Db()
    {
        var profile = new CustomerProfile
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            FullName = "Jane Doe",
            Address = "42 Oak Street",
            PhoneNumber = "+15551112222"
        };

        await _repo.AddCustomerProfileAsync(profile);
        await _repo.SaveCustomerProfileChangesAsync();

        var found = await _dbContext.CustomerProfiles.FindAsync(profile.Id);

        Assert.NotNull(found);
        Assert.Equal("Jane Doe", found!.FullName);
    }

    [Fact]
    public async Task GetByUserIdAsync_Should_Return_Correct_Profile()
    {
        var userId = Guid.NewGuid();

        var profile = new CustomerProfile
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            FullName = "Carlos Rivera",
            Address = "123 Birch Rd",
            PhoneNumber = "+15550001111"
        };

        await _dbContext.CustomerProfiles.AddAsync(profile);
        await _dbContext.SaveChangesAsync();

        var result = await _repo.GetCustomerProfileByUserIdAsync(userId);

        Assert.NotNull(result);
        Assert.Equal("Carlos Rivera", result!.FullName);
    }

    [Fact]
    public async Task GetByUserIdAsync_Should_Return_Null_When_NotFound()
    {
        var result = await _repo.GetCustomerProfileByUserIdAsync(Guid.NewGuid());
        Assert.Null(result);
    }
}
