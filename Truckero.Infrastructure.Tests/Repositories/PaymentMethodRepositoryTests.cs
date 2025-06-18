using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.IO;
using System.Threading.Tasks;
using Truckero.Core.Entities;
using Truckero.Diagnostics.Data;
using Truckero.Infrastructure.Data;
using Truckero.Infrastructure.Repositories;
using Truckero.Infrastructure.Tests.Helpers;
using Xunit;

namespace Truckero.Infrastructure.Tests.Repositories;

public class PaymentMethodRepositoryTests : IDisposable
{
    private readonly AppDbContext _dbContext;
    private readonly PaymentMethodRepository _repo;
    private readonly SqliteConnection _connection;
    private readonly User _user;
    private readonly PaymentMethodType _cardPaymentMethodType;
    private readonly PaymentMethodType _bankPaymentMethodType;

    public PaymentMethodRepositoryTests()
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

        // Use NullLogger for tests
        var logger = NullLogger<PaymentMethodRepository>.Instance;
        _repo = new PaymentMethodRepository(_dbContext, logger);
        
        // Use mock customer user from MockUserTestData
        _user = MockUserTestData.CustomerUser;

        // Ensure user exists using helper
        _dbContext.EnsureUserExistsAsync(_user).GetAwaiter().GetResult();
        
        // Use payment method types from MockCustomerTestData
        _cardPaymentMethodType = MockCustomerTestData.CardPaymentMethodType;
        _bankPaymentMethodType = MockCustomerTestData.BankPaymentMethodType;
        
        // Ensure payment method types exist
        _dbContext.EnsurePaymentMethodTypeExistsAsync(_cardPaymentMethodType).GetAwaiter().GetResult();
        _dbContext.EnsurePaymentMethodTypeExistsAsync(_bankPaymentMethodType).GetAwaiter().GetResult();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public async Task AddAsync_Should_Persist_PaymentMethod()
    {
        // Create a customer with payment methods
        var customerWithPayments = MockCustomerTestData.CreateCustomerWithPaymentMethods();
        
        // Use one of the payment methods, but assign to our test user
        var paymentMethod = customerWithPayments.PaymentMethods.First();
        paymentMethod.UserId = _user.Id;

        // Act
        await _repo.AddPaymentMethodAsync(paymentMethod);

        // Assert
        var stored = await _dbContext.PaymentMethods.FindAsync(paymentMethod.Id);
        Assert.NotNull(stored);
        Assert.Equal(paymentMethod.Last4, stored!.Last4);
        Assert.Equal(paymentMethod.TokenizedId, stored.TokenizedId);
    }

    [Fact]
    public async Task GetByUserIdAsync_Should_Return_All_For_User()
    {
        // Arrange - Create two payment methods for the user
        var paymentMethod1 = new PaymentMethod
        {
            Id = Guid.NewGuid(),
            UserId = _user.Id,
            PaymentMethodTypeId = _cardPaymentMethodType.Id,
            TokenizedId = "tok_test_visa_123",
            Last4 = "1111",
            IsDefault = true,
            CreatedAt = DateTime.UtcNow.AddDays(-5)
        };

        var paymentMethod2 = new PaymentMethod
        {
            Id = Guid.NewGuid(),
            UserId = _user.Id,
            PaymentMethodTypeId = _bankPaymentMethodType.Id,
            TokenizedId = "ba_test_checking_456",
            Last4 = "2222",
            IsDefault = false,
            CreatedAt = DateTime.UtcNow.AddDays(-3)
        };

        // Remove any existing payment methods for this user
        var existingMethods = await _dbContext.PaymentMethods
            .Where(p => p.UserId == _user.Id)
            .ToListAsync();
        
        if (existingMethods.Any())
        {
            _dbContext.PaymentMethods.RemoveRange(existingMethods);
            await _dbContext.SaveChangesAsync();
        }
        
        _dbContext.PaymentMethods.AddRange(paymentMethod1, paymentMethod2);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repo.GetPaymentMethodsByUserIdAsync(_user.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Contains(result, p => p.Last4 == "1111");
        Assert.Contains(result, p => p.Last4 == "2222");
    }

    [Fact]
    public async Task GetDefaultByUserIdAsync_Should_Return_Only_Default()
    {
        // Arrange
        var customerWithPayments = MockCustomerTestData.CreateCustomerWithPaymentMethods();
        
        // Get the payment methods and modify them to use our test user
        var defaultPaymentMethod = customerWithPayments.PaymentMethods.First(p => p.IsDefault);
        var nonDefaultPaymentMethod = customerWithPayments.PaymentMethods.First(p => !p.IsDefault);
        
        defaultPaymentMethod.UserId = _user.Id;
        nonDefaultPaymentMethod.UserId = _user.Id;

        // Remove any existing payment methods for this user
        var existingMethods = await _dbContext.PaymentMethods
            .Where(p => p.UserId == _user.Id)
            .ToListAsync();
        
        if (existingMethods.Any())
        {
            _dbContext.PaymentMethods.RemoveRange(existingMethods);
            await _dbContext.SaveChangesAsync();
        }
        
        _dbContext.PaymentMethods.AddRange(defaultPaymentMethod, nonDefaultPaymentMethod);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repo.GetDefaultPaymentMethodByUserIdAsync(_user.Id);

        // Assert
        Assert.NotNull(result);
        Assert.True(result!.IsDefault);
        Assert.Equal(defaultPaymentMethod.Id, result.Id);
        Assert.Equal(defaultPaymentMethod.Last4, result.Last4);
    }

    [Fact]
    public async Task DeleteAsync_Should_Remove_Entry()
    {
        // Arrange
        var paymentMethod = new PaymentMethod
        {
            Id = Guid.NewGuid(),
            UserId = _user.Id,
            PaymentMethodTypeId = _cardPaymentMethodType.Id,
            TokenizedId = "tok_test_delete_789",
            Last4 = "9999",
            IsDefault = false,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.PaymentMethods.Add(paymentMethod);
        await _dbContext.SaveChangesAsync();

        // Act
        await _repo.DeletePaymentMethodAsync(paymentMethod.Id);

        // Assert
        var result = await _dbContext.PaymentMethods.FindAsync(paymentMethod.Id);
        Assert.Null(result);
    }

    [Fact]
    public async Task AddAsync_Should_Persist_MetadataJson()
    {
        // Arrange - Create a card metadata object
        var cardMetadata = new CardMetadata
        {
            Brand = "Visa",
            ExpiryMonth = "12",
            ExpiryYear = (DateTime.UtcNow.Year + 3).ToString(),
            Country = "US"
        };
        
        var paymentMethod = new PaymentMethod
        {
            Id = Guid.NewGuid(),
            UserId = _user.Id,
            PaymentMethodTypeId = _cardPaymentMethodType.Id,
            TokenizedId = "tok_test_metadata_101",
            Last4 = "4242",
            IsDefault = false,
            CreatedAt = DateTime.UtcNow,
            MetadataJson = System.Text.Json.JsonSerializer.Serialize(cardMetadata)
        };

        // Act
        await _repo.AddPaymentMethodAsync(paymentMethod);

        // Assert
        var stored = await _dbContext.PaymentMethods.FindAsync(paymentMethod.Id);
        Assert.NotNull(stored);
        Assert.Contains("Visa", stored!.MetadataJson);
        Assert.Contains("ExpiryMonth", stored.MetadataJson);
    }

    [Fact]
    public async Task GetMetadata_Should_Deserialize_CardMetadata()
    {
        // Arrange
        var customerWithPayments = MockCustomerTestData.CreateCustomerWithPaymentMethods();
        var creditCardPayment = customerWithPayments.PaymentMethods.First(p => p.PaymentMethodTypeId == MockCustomerTestData.Ids.CardPaymentMethodTypeId);
        
        creditCardPayment.UserId = _user.Id;
        
        _dbContext.PaymentMethods.Add(creditCardPayment);
        await _dbContext.SaveChangesAsync();

        // Act
        var stored = await _repo.GetPaymentMethodsByUserIdAsync(_user.Id);
        var paymentMethod = stored.First(p => p.Id == creditCardPayment.Id);
        
        // Assert
        Assert.NotNull(paymentMethod);
        var metadata = paymentMethod.GetMetadata<CardMetadata>();
        Assert.NotNull(metadata);
        Assert.Equal("Visa", metadata!.Brand);
        Assert.Equal("12", metadata.ExpiryMonth);
        Assert.NotNull(metadata.ExpiryYear);
        Assert.Equal("US", metadata.Country);
    }
}
