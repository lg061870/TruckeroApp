// 📦 Repository Unit Tests: AuthToken + User Repositories

using Truckero.Core.Entities;
using Truckero.Infrastructure.Data;
using Truckero.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Truckero.Infrastructure.Tests;

public class AuthTokenRepositoryTests
{
    private readonly AuthTokenRepository _repo;
    private readonly AppDbContext _dbContext;

    public AuthTokenRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _dbContext = new AppDbContext(options);
        _repo = new AuthTokenRepository(_dbContext);
    }

    [Fact]
    public async Task AddAsync_Should_Persist_AuthToken()
    {
        var token = new AuthToken
        {
            UserId = Guid.NewGuid(),
            RefreshToken = "token-123",
            AccessToken = "access-abc",
            IssuedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        await _repo.AddAsync(token);

        var result = await _repo.GetByRefreshTokenAsync("token-123");
        Assert.NotNull(result);
        Assert.Equal("token-123", result!.RefreshToken);
    }

    [Fact]
    public async Task UpdateAsync_Should_Modify_Token()
    {
        var token = new AuthToken
        {
            UserId = Guid.NewGuid(),
            RefreshToken = "old-token",
            AccessToken = "old-access",
            IssuedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(1)
        };

        await _repo.AddAsync(token);
        token.RefreshToken = "new-token";
        await _repo.UpdateAsync(token);

        var result = await _repo.GetByRefreshTokenAsync("new-token");
        Assert.NotNull(result);
        Assert.Equal("new-token", result!.RefreshToken);
    }

    [Fact]
    public async Task DeleteAsync_Should_Remove_Token()
    {
        var token = new AuthToken
        {
            UserId = Guid.NewGuid(),
            RefreshToken = "del-token",
            AccessToken = "del-access",
            IssuedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(1)
        };

        await _repo.AddAsync(token);
        await _repo.DeleteAsync(token);

        var result = await _repo.GetByRefreshTokenAsync("del-token");
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByUserIdAsync_Should_Return_Correct_Token()
    {
        var userId = Guid.NewGuid();
        var token = new AuthToken
        {
            UserId = userId,
            RefreshToken = "uid-token",
            AccessToken = "uid-access",
            IssuedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(1)
        };

        await _repo.AddAsync(token);

        var result = await _repo.GetByUserIdAsync(userId);
        Assert.NotNull(result);
        Assert.Equal(userId, result!.UserId);
    }
}
