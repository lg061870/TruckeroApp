// 📦 Repository Unit Tests: AuthToken + User Repositories
using Truckero.Core.Entities;
using Truckero.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Truckero.Infrastructure.Tests;

public class UserRepositoryTests
{
    private readonly UserRepository _repo;
    private readonly AppDbContext _dbContext;

    public UserRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _dbContext = new AppDbContext(options);
        _repo = new UserRepository(_dbContext);
    }

    [Fact]
    public async Task AddAsync_And_Save_Should_Create_User()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            B2CObjectId = Guid.NewGuid().ToString(), // ✅ REQUIRED FIELD
        };

        await _repo.AddAsync(user);
        await _repo.SaveChangesAsync();

        var found = await _repo.GetByEmailAsync("test@example.com");

        Assert.NotNull(found);
        Assert.Equal("test@example.com", found!.Email);
    }

    [Fact]
    public async Task GetByIdAsync_Should_Find_User()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "byid@example.com",
            B2CObjectId = "test-b2c-id" // ✅ Required field
        };

        await _repo.AddAsync(user);
        await _repo.SaveChangesAsync();

        var found = await _repo.GetByIdAsync(user.Id);

        Assert.NotNull(found);
        Assert.Equal(user.Id, found!.Id);
    }

}
