using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Truckero.Core.Entities;
using Truckero.Infrastructure.Data;
using Truckero.Infrastructure.Repositories;
using Xunit;

namespace Truckero.Infrastructure.Tests.Repositories;

public class OnboardingRepositoryTests
{
    private readonly AppDbContext _dbContext;
    private readonly OnboardingProgressRepository _repo;
    private readonly SqliteConnection _connection;
    private readonly Guid _userId;

    public OnboardingRepositoryTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;
        _dbContext = new AppDbContext(options);
        _dbContext.Database.EnsureCreated();
        _repo = new OnboardingProgressRepository(_dbContext);
        _userId = Guid.NewGuid();
        _dbContext.Users.Add(new User {
            Id = _userId,
            Email = "onboard@example.com",
            CreatedAt = DateTime.UtcNow
        });
        _dbContext.SaveChanges();
    }

    [Fact]
    public async Task AddOrUpdateAsync_Should_Persist_OnboardingProgress()
    {
        var progress = new OnboardingProgress
        {
            UserId = _userId,
            StepCurrent = "step2",
            Completed = false
        };
        await _repo.AddOrUpdateAsync(progress);
        await _repo.SaveChangesAsync();
        var stored = await _dbContext.Onboardings.FindAsync(_userId);
        Assert.NotNull(stored);
        Assert.Equal("step2", stored!.StepCurrent);
    }

    [Fact]
    public async Task GetByUserIdAsync_Should_Return_Progress()
    {
        var progress = new OnboardingProgress
        {
            UserId = _userId,
            StepCurrent = "step3",
            Completed = true
        };
        _dbContext.Onboardings.Add(progress);
        await _dbContext.SaveChangesAsync();
        var result = await _repo.GetByUserIdAsync(_userId);
        Assert.NotNull(result);
        Assert.True(result!.Completed);
    }

    [Fact]
    public async Task GetByUserIdAsync_Should_Return_Null_If_NotFound()
    {
        var result = await _repo.GetByUserIdAsync(Guid.NewGuid());
        Assert.Null(result);
    }

    ~OnboardingRepositoryTests()
    {
        _dbContext?.Dispose();
        _connection?.Dispose();
    }
}
