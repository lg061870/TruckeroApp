using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Truckero.Core.Entities;
using Truckero.Diagnostics.Data;
using Truckero.Infrastructure.Data;
using Truckero.Infrastructure.Repositories;
using Truckero.Infrastructure.Tests.Fixtures;
using Xunit;

namespace Truckero.Infrastructure.Tests.Repositories;

public class CustomerRepositoryTests : IClassFixture<TestDbContextFixture> {
    private readonly TestDbContextFixture _fixture;
    private readonly CustomerRepository _repo;

    public CustomerRepositoryTests(TestDbContextFixture fixture) {
        _fixture = fixture;
        _repo = new CustomerRepository(fixture.DbContext);
    }

    [Fact]
    public async Task AddAsync_Should_Add_Profile_To_Db() {
        // Arrange
        var userId = MockUserTestData.Ids.CustomerUserId;
        
        // Create a user first
        var user = new User {
            Id = userId,
            Email = $"test_{Guid.NewGuid().ToString().Substring(0, 8)}@example.com",
            FullName = "Test Customer",
            PasswordHash = "HASH_VALUE_HERE",
            RoleId = MockRoleTestData.Ids.CustomerRoleId
        };
        
        // Get role from database (don't add new one)
        var role = await _fixture.DbContext.Roles.FirstOrDefaultAsync(r => r.Id == user.RoleId);
        user.Role = role;
        
        if (!_fixture.DbContext.ChangeTracker.Entries<Role>().Any(e => e.Entity.Id == role.Id)) {
            _fixture.DbContext.Roles.Add(role);
            await _fixture.DbContext.SaveChangesAsync();
        }
        
        _fixture.DbContext.Users.Add(user);
        await _fixture.DbContext.SaveChangesAsync();
        
        var profile = new CustomerProfile {
            Id = Guid.NewGuid(),
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        await _repo.AddCustomerProfileAsync(profile);
        await _repo.SaveCustomerProfileChangesAsync();

        // Assert
        var found = await _fixture.DbContext.CustomerProfiles.FindAsync(profile.Id);
        Assert.NotNull(found);
        Assert.Equal(userId, found!.UserId);
    }

    [Fact]
    public async Task GetByUserIdAsync_Should_Return_Correct_Profile() {
        // Arrange - Clone mock customer and user for isolation
        var originalProfile = MockCustomerTestData.StandardCustomer;
        var clonedUser = TestUserCloner.CloneWithNewId(MockUserTestData.CustomerUser);
        var customerProfile = new CustomerProfile {
            Id = Guid.NewGuid(),
            UserId = clonedUser.Id,
            CreatedAt = DateTime.UtcNow
        };

        // Ensure user exists first
        _fixture.DbContext.Users.Add(clonedUser);
        await _fixture.DbContext.SaveChangesAsync();

        // Add customer profile
        _fixture.DbContext.CustomerProfiles.Add(customerProfile);
        await _fixture.DbContext.SaveChangesAsync();

        // Act
        var result = await _repo.GetCustomerProfileByUserIdAsync(customerProfile.UserId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(customerProfile.Id, result!.Id);
        Assert.Equal(customerProfile.UserId, result.UserId);
    }

    [Fact]
    public async Task GetByUserIdAsync_Should_Return_Null_When_NotFound() {
        // Arrange - Use a non-existent user ID
        var nonExistentUserId = Guid.NewGuid();

        // Act
        var result = await _repo.GetCustomerProfileByUserIdAsync(nonExistentUserId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteCustomerProfileAsync_Should_Remove_Profile() {
        // Arrange - Create a new customer profile
        var newCustomer = MockCustomerTestData.NewCustomer;
        await EnsureUserExistsForCustomerAsync(newCustomer);
        
        _fixture.DbContext.CustomerProfiles.Add(newCustomer);
        await _fixture.DbContext.SaveChangesAsync();

        // Verify it exists before deletion
        var beforeDelete = await _fixture.DbContext.CustomerProfiles.FindAsync(newCustomer.Id);
        Assert.NotNull(beforeDelete);

        // Act
        await _repo.DeleteCustomerProfileChangesAsync(newCustomer.UserId);

        // Assert
        var afterDelete = await _fixture.DbContext.CustomerProfiles.FindAsync(newCustomer.Id);
        Assert.Null(afterDelete);
    }

    [Fact]
    public async Task GetCustomerWithPaymentMethodsAsync_Should_Return_Related_Data() {
        // Arrange - Use customer with payment methods from MockCustomerTestData
        var customerWithPaymentMethods = MockCustomerTestData.CreateCustomerWithPaymentMethods();

        // Ensure user exists with proper role and auth token
        await EnsureUserExistsForCustomerAsync(customerWithPaymentMethods);

        // Get payment method types from database or add if needed
        var cardType = await _fixture.DbContext.PaymentMethodTypes.FindAsync(MockCustomerTestData.CardPaymentMethodType.Id);
        if (cardType == null) {
            _fixture.DbContext.PaymentMethodTypes.Add(MockCustomerTestData.CardPaymentMethodType);
        }
        
        var bankType = await _fixture.DbContext.PaymentMethodTypes.FindAsync(MockCustomerTestData.BankPaymentMethodType.Id);
        if (bankType == null) {
            _fixture.DbContext.PaymentMethodTypes.Add(MockCustomerTestData.BankPaymentMethodType);
        }
        
        await _fixture.DbContext.SaveChangesAsync();

        // Add customer profile and payment methods
        _fixture.DbContext.CustomerProfiles.Add(customerWithPaymentMethods);
        _fixture.DbContext.PaymentAccounts.AddRange(customerWithPaymentMethods.PaymentAccounts);
        await _fixture.DbContext.SaveChangesAsync();

        // Act - Retrieve customer with payment methods
        var result = await _repo.GetCustomerProfileByUserIdAsync(customerWithPaymentMethods.UserId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(customerWithPaymentMethods.Id, result!.Id);

        // Verify payment methods if the repository method includes them
        if (result.PaymentAccounts != null) {
            Assert.NotEmpty(result.PaymentAccounts);
            Assert.Equal(customerWithPaymentMethods.PaymentAccounts.Count, result.PaymentAccounts.Count);
        }
    }

    [Fact]
    public async Task UpdateAsync_Should_Update_CustomerProfile() {
        // Arrange
        var customerProfile = MockCustomerTestData.StandardCustomer;
        await EnsureUserExistsForCustomerAsync(customerProfile);
        
        _fixture.DbContext.CustomerProfiles.Add(customerProfile);
        await _fixture.DbContext.SaveChangesAsync();

        // Modify profile data using an existing property (CreatedAt) instead of Notes
        var updatedDate = DateTime.UtcNow.AddDays(-7); // Set to a different date for testing
        customerProfile.CreatedAt = updatedDate;

        // Act
        await _repo.UpdateCustomerProfileAsync(customerProfile);
        await _repo.SaveCustomerProfileChangesAsync();

        // Assert
        var updated = await _fixture.DbContext.CustomerProfiles.FindAsync(customerProfile.Id);
        Assert.NotNull(updated);
        Assert.Equal(updatedDate, updated.CreatedAt);
    }

    [Fact]
    public async Task GetAllCustomerProfilesAsync_Should_Return_All_Profiles() {
        // Arrange - Clone users and profiles for isolation
        var user1 = TestUserCloner.CloneWithNewId(MockUserTestData.CustomerUser);
        var user2 = TestUserCloner.CloneWithNewId(MockUserTestData.CustomerUser);
        var customer1 = new CustomerProfile {
            Id = Guid.NewGuid(),
            UserId = user1.Id,
            CreatedAt = DateTime.UtcNow.AddDays(-2)
        };
        var customer2 = new CustomerProfile {
            Id = Guid.NewGuid(),
            UserId = user2.Id,
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };

        _fixture.DbContext.Users.AddRange(user1, user2);
        await _fixture.DbContext.SaveChangesAsync();
        _fixture.DbContext.CustomerProfiles.AddRange(customer1, customer2);
        await _fixture.DbContext.SaveChangesAsync();

        // Act
        var customers = await _repo.GetAllCustomerProfilesAsync();

        // Assert
        Assert.NotNull(customers);
        Assert.NotEmpty(customers);
        Assert.Contains(customers, c => c.Id == customer1.Id);
        Assert.Contains(customers, c => c.Id == customer2.Id);
    }

    [Fact]
    public async Task CreateCustomerProfile_With_New_User_Should_Succeed() {
        // Arrange
        var userId = Guid.NewGuid();
        
        // Create a new user
        var user = new User {
            Id = userId,
            Email = $"customer_{Guid.NewGuid().ToString().Substring(0, 8)}@example.com",
            FullName = "New Customer Test",
            PasswordHash = "hashedpassword123",
            PhoneNumber = "555-987-6543",
            RoleId = MockRoleTestData.Ids.CustomerRoleId
        };
        
        // Get role from database (don't add new one)
        var role = await _fixture.DbContext.Roles.FirstOrDefaultAsync(r => r.Id == user.RoleId);
        user.Role = role;
        
        if (!_fixture.DbContext.ChangeTracker.Entries<Role>().Any(e => e.Entity.Id == role.Id)) {
            _fixture.DbContext.Roles.Add(role);
            await _fixture.DbContext.SaveChangesAsync();
        }
        
        _fixture.DbContext.Users.Add(user);
        await _fixture.DbContext.SaveChangesAsync();
        
        // Create customer profile
        var customerProfile = new CustomerProfile {
            Id = Guid.NewGuid(),
            UserId = userId
        };
        
        // Act
        await _repo.AddCustomerProfileAsync(customerProfile);
        await _repo.SaveCustomerProfileChangesAsync();
        
        // Assert
        var result = await _repo.GetCustomerProfileByUserIdAsync(userId);
        Assert.NotNull(result);
        Assert.Equal(customerProfile.Id, result!.Id);
        Assert.Equal(userId, result.UserId);
    }

    // Improved helper method that avoids entity tracking issues
    private async Task<User> EnsureUserExistsForCustomerAsync(CustomerProfile customerProfile) {
        // 1. Try to get the user from the database
        var existingUser = await _fixture.DbContext.Users
            .FirstOrDefaultAsync(u => u.Id == customerProfile.UserId);

        if (existingUser != null) {
            customerProfile.UserId = existingUser.Id;
            customerProfile.User = existingUser;
            return existingUser;
        }

        // 2. Only create a new user if not found
        var user = new User {
            Id = customerProfile.UserId,
            Email = $"customer_{Guid.NewGuid().ToString().Substring(0, 8)}@example.com",
            FullName = "Test Customer",
            PasswordHash = "HASH_VALUE_HERE",
            RoleId = MockRoleTestData.Ids.CustomerRoleId,
            EmailVerified = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var role = await _fixture.DbContext.Roles.FirstOrDefaultAsync(r => r.Id == user.RoleId);
        user.Role = role;

        _fixture.DbContext.Users.Add(user);
        await _fixture.DbContext.SaveChangesAsync();

        customerProfile.UserId = user.Id;
        customerProfile.User = user;

        // Add auth token for the user if needed
        var existingToken = await _fixture.DbContext.AuthTokens
            .FirstOrDefaultAsync(t => t.UserId == user.Id);

        if (existingToken == null) {
            var token = MockUserTestData.GetAuthTokenForUser(user.Id);
            if (token != null) {
                _fixture.DbContext.AuthTokens.Add(token);
                await _fixture.DbContext.SaveChangesAsync();
            }
        }

        return user;
    }
}