using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Truckero.Core.Entities;
using Truckero.Infrastructure.Data;
using Truckero.Infrastructure.Repositories;

namespace Truckero.Infrastructure.Tests.Repositories;

public class PaymentMethodRepositoryTests
{
    private readonly AppDbContext _dbContext;
    private readonly PaymentMethodRepository _repo;
    private readonly Guid _userId;
    private readonly Guid _typeId;

    public PaymentMethodRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _dbContext = new AppDbContext(options);
        _repo = new PaymentMethodRepository(_dbContext);
        _userId = Guid.NewGuid();
        _typeId = Guid.NewGuid();

        _dbContext.PaymentMethodTypes.Add(new PaymentMethodType
        {
            Id = _typeId,
            Name = "Card",
            Description = "Credit card",
            IsForPayment = true,
            IsForPayout = false
        });

        _dbContext.SaveChanges();
    }

    [Fact]
    public async Task AddAsync_Should_Persist_PaymentMethod()
    {
        var method = new PaymentMethod
        {
            Id = Guid.NewGuid(),
            UserId = _userId,
            PaymentMethodTypeId = _typeId,
            Last4 = "1234",
            IsDefault = false,
            TokenizedId = "tok_test_123"
        };

        await _repo.AddAsync(method);

        var stored = await _dbContext.PaymentMethods.FindAsync(method.Id);
        Assert.NotNull(stored);
        Assert.Equal("1234", stored!.Last4);
    }

    [Fact]
    public async Task GetByUserIdAsync_Should_Return_All_For_User()
    {
        var m1 = new PaymentMethod
        {
            Id = Guid.NewGuid(),
            UserId = _userId,
            PaymentMethodTypeId = _typeId,
            Last4 = "1111",
            TokenizedId = "tok_1"
        };
        var m2 = new PaymentMethod
        {
            Id = Guid.NewGuid(),
            UserId = _userId,
            PaymentMethodTypeId = _typeId,
            Last4 = "2222",
            TokenizedId = "tok_2"
        };

        _dbContext.PaymentMethods.AddRange(m1, m2);
        await _dbContext.SaveChangesAsync();

        var result = await _repo.GetByUserIdAsync(_userId);
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetDefaultByUserIdAsync_Should_Return_Only_Default()
    {
        var m1 = new PaymentMethod
        {
            Id = Guid.NewGuid(),
            UserId = _userId,
            PaymentMethodTypeId = _typeId,
            Last4 = "3333",
            IsDefault = false,
            TokenizedId = "tok_3"
        };
        var m2 = new PaymentMethod
        {
            Id = Guid.NewGuid(),
            UserId = _userId,
            PaymentMethodTypeId = _typeId,
            Last4 = "4444",
            IsDefault = true,
            TokenizedId = "tok_4"
        };

        _dbContext.PaymentMethods.AddRange(m1, m2);
        await _dbContext.SaveChangesAsync();

        var result = await _repo.GetDefaultByUserIdAsync(_userId);
        Assert.NotNull(result);
        Assert.True(result!.IsDefault);
        Assert.Equal("4444", result.Last4);
    }

    [Fact]
    public async Task DeleteAsync_Should_Remove_Entry()
    {
        var method = new PaymentMethod
        {
            Id = Guid.NewGuid(),
            UserId = _userId,
            PaymentMethodTypeId = _typeId,
            Last4 = "9999",
            TokenizedId = "tok_5"
        };

        _dbContext.PaymentMethods.Add(method);
        await _dbContext.SaveChangesAsync();

        await _repo.DeleteAsync(method.Id);

        var result = await _dbContext.PaymentMethods.FindAsync(method.Id);
        Assert.Null(result);
    }

    [Fact]
    public async Task AddAsync_Should_Persist_MetadataJson()
    {
        var metadata = new
        {
            Cardholder = "John Doe",
            Expiry = "12/29"
        };

        var method = new PaymentMethod
        {
            Id = Guid.NewGuid(),
            UserId = _userId,
            PaymentMethodTypeId = _typeId,
            TokenizedId = "tok_meta_1",
            MetadataJson = JsonSerializer.Serialize(metadata)
        };

        await _repo.AddAsync(method);

        var stored = await _dbContext.PaymentMethods.FindAsync(method.Id);
        Assert.NotNull(stored);
        Assert.Contains("Cardholder", stored!.MetadataJson);
    }

    [Fact]
    public async Task GetMetadata_Should_Deserialize_CardMetadata()
    {
        var metadata = new CardMetadata
        {
            Brand = "Visa",
            ExpiryMonth = "12",
            ExpiryYear = "2029",
            Country = "US"
        };

        var method = new PaymentMethod
        {
            Id = Guid.NewGuid(),
            UserId = _userId,
            PaymentMethodTypeId = _typeId,
            TokenizedId = "tok_meta_2",
            MetadataJson = JsonSerializer.Serialize(metadata)
        };

        _dbContext.PaymentMethods.Add(method);
        await _dbContext.SaveChangesAsync();

        var stored = await _repo.GetByUserIdAsync(_userId);
        var parsed = stored.First().GetMetadata<CardMetadata>();

        Assert.Equal("Visa", parsed.Brand);
        Assert.Equal("12", parsed.ExpiryMonth);
        Assert.Equal("2029", parsed.ExpiryYear);
        Assert.Equal("US", parsed.Country);
    }
}
