using Truckero.Core.Entities;
using Truckero.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Truckero.Infrastructure.Repositories;
using Truckero.Core.Enums;
using Truckero.Diagnostics.Seeders;

namespace Truckero.Infrastructure.Tests.Repositories;

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

        // ✅ Seed required roles into in-memory DB
        _dbContext.Roles.AddRange(TestSeedHelpers.AllRoles());
        _dbContext.SaveChanges();

        _repo = new UserRepository(_dbContext);
    }

    [Fact]
    public async Task AddAsync_And_Save_Should_Create_User()
    {
        // Arrange
        var role = _dbContext.Roles.First(r => r.Name == RoleType.Customer.ToString());

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            RoleId = role.Id,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        await _repo.AddAsync(user);
        await _repo.SaveChangesAsync();

        // Assert
        var found = await _repo.GetByEmailAsync("test@example.com");

        Assert.NotNull(found);
        Assert.Equal("test@example.com", found!.Email);
        Assert.Equal(role.Id, found.RoleId);
    }

    [Fact]
    public async Task GetByIdAsync_Should_Find_User()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "byid@example.com",
            Role = _dbContext.Roles.First(r => r.Name == RoleType.Customer.ToString()),
            CreatedAt = DateTime.UtcNow
        };

        await _repo.AddAsync(user);
        await _repo.SaveChangesAsync();

        // Act
        var found = await _repo.GetByIdAsync(user.Id);

        // Assert
        Assert.NotNull(found);
        Assert.Equal(user.Id, found!.Id);
    }

}
