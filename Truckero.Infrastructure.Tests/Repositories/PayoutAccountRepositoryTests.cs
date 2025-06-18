using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.IO;
using Truckero.Core.Entities;
using Truckero.Infrastructure.Data;
using Truckero.Infrastructure.Repositories;
using Xunit;

namespace Truckero.Infrastructure.Tests.Repositories;

public class PayoutAccountRepositoryTests : IDisposable
{
    private readonly AppDbContext _dbContext;
    private readonly PayoutAccountRepository _repo;
    private readonly SqliteConnection _connection;
    private readonly Guid _userId;
    private readonly Guid _typeId;

    public PayoutAccountRepositoryTests()
    {
        // Use the persisted SQLite database file created by TestTemplateBuilder
        var dbPath = Path.Combine("..", "..", "..", "..", "Truckero.Diagnostics", "CloneDBs", "TestTemplate.sqlite");

        // Ensure the file exists, otherwise tests will fail
        if (!File.Exists(dbPath))
        {
            throw new FileNotFoundException($"The SQLite database file was not found at: {dbPath}. Please run TestTemplateBuilder first to create it.");
        }

        // Connect to the file-based SQLite database
        _connection = new SqliteConnection($"DataSource={dbPath}");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        _dbContext = new AppDbContext(options);
        _repo = new PayoutAccountRepository(_dbContext, NullLogger<PayoutAccountRepository>.Instance);
        _userId = Guid.NewGuid();
        
        // Check if we already have a payment method type for payouts
        var paymentMethodType = _dbContext.PaymentMethodTypes.FirstOrDefault(p => p.IsForPayout);
        
        // If no suitable payment method type found, create one
        if (paymentMethodType == null)
        {
            paymentMethodType = new PaymentMethodType 
            {
                Id = Guid.NewGuid(),
                Name = "Bank",
                IsForPayment = true,
                IsForPayout = true
            };
            _dbContext.PaymentMethodTypes.Add(paymentMethodType);
            _dbContext.SaveChanges();
        }
        
        _typeId = paymentMethodType.Id;
    }

    [Fact]
    public async Task AddPayoutAccountAsync_Should_Persist_Account()
    {
        var account = new PayoutAccount
        {
            Id = Guid.NewGuid(),
            UserId = _userId,
            PaymentMethodTypeId = _typeId,
            AccountNumberLast4 = "1234",
            IsDefault = true
        };
        await _repo.AddPayoutAccountAsync(account);
        await _dbContext.SaveChangesAsync();
        var stored = await _dbContext.PayoutAccounts.FindAsync(account.Id);
        Assert.NotNull(stored);
        Assert.Equal("1234", stored!.AccountNumberLast4);
    }

    [Fact]
    public async Task GetPayoutAccountsByUserIdAsync_Should_Return_All_For_User()
    {
        var a1 = new PayoutAccount
        {
            Id = Guid.NewGuid(),
            UserId = _userId,
            PaymentMethodTypeId = _typeId,
            AccountNumberLast4 = "1111"
        };
        var a2 = new PayoutAccount
        {
            Id = Guid.NewGuid(),
            UserId = _userId,
            PaymentMethodTypeId = _typeId,
            AccountNumberLast4 = "2222"
        };
        _dbContext.PayoutAccounts.AddRange(a1, a2);
        await _dbContext.SaveChangesAsync();
        var result = await _repo.GetPayoutAccountsByUserIdAsync(_userId);
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetDefaultPayoutAccountByUserIdAsync_Should_Return_Only_Default()
    {
        var a1 = new PayoutAccount
        {
            Id = Guid.NewGuid(),
            UserId = _userId,
            PaymentMethodTypeId = _typeId,
            AccountNumberLast4 = "3333",
            IsDefault = false
        };
        var a2 = new PayoutAccount
        {
            Id = Guid.NewGuid(),
            UserId = _userId,
            PaymentMethodTypeId = _typeId,
            AccountNumberLast4 = "4444",
            IsDefault = true
        };
        _dbContext.PayoutAccounts.AddRange(a1, a2);
        await _dbContext.SaveChangesAsync();
        var result = await _repo.GetDefaultPayoutAccountByUserIdAsync(_userId);
        Assert.NotNull(result);
        Assert.True(result!.IsDefault);
        Assert.Equal("4444", result.AccountNumberLast4);
    }

    [Fact]
    public async Task DeletePayoutAccountAsync_Should_Remove_Entry()
    {
        var account = new PayoutAccount
        {
            Id = Guid.NewGuid(),
            UserId = _userId,
            PaymentMethodTypeId = _typeId,
            AccountNumberLast4 = "9999"
        };
        _dbContext.PayoutAccounts.Add(account);
        await _dbContext.SaveChangesAsync();
        await _repo.DeletePayoutAccountAsync(account.Id);
        await _dbContext.SaveChangesAsync();
        var result = await _dbContext.PayoutAccounts.FindAsync(account.Id);
        Assert.Null(result);
    }

    public void Dispose()
    {
        _dbContext?.Dispose();
        _connection?.Dispose();
    }
}
