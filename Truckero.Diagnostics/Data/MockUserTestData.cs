using System;
using System.Collections.Generic;
using Truckero.Core.Entities;

namespace Truckero.Diagnostics.Data;

/// <summary>
/// Provides mock user data objects for unit tests
/// </summary>
public static class MockUserTestData {
    // Nested class for commonly used test IDs
    public static class Ids {
        // User IDs
        public static readonly Guid CustomerUserId = new Guid("9C8B6DEF-32D2-4A30-B8B9-AB3D5843C8A0");
        public static readonly Guid DriverUserId = new Guid("5B92DA9A-E83C-41D5-A9A8-D39A4B7AABD7");
        public static readonly Guid StoreClerkUserId = new Guid("C7B12DCE-9B89-45F5-B299-EE67A8B88F20");
        public static readonly Guid AdminUserId = new Guid("1A53ED9E-D697-4C64-84DF-261BA4A99DFC");
        public static readonly Guid UnverifiedUserId = Guid.Parse("ad47c51f-0bb9-4239-8706-2d9c19aca437");
        public static readonly Guid InactiveUserId = Guid.Parse("e5c2e84b-3648-4d3a-80f3-591b3d5a7cdc");

        // Role IDs (FIXED to match AppDbContext and MockRoleTestData)
        public static readonly Guid CustomerRoleId = Guid.Parse("00000000-0000-0000-0000-000000000002");
        public static readonly Guid DriverRoleId = Guid.Parse("00000000-0000-0000-0000-000000000003");
        public static readonly Guid StoreClerkRoleId = Guid.Parse("00000000-0000-0000-0000-000000000004");
        public static readonly Guid AdminRoleId = Guid.Parse("00000000-0000-0000-0000-000000000005");

        // Auth Token IDs
        public static readonly Guid CustomerAuthTokenId = Guid.Parse("E46D39F3-8910-4D8F-B1D1-C67D9B35D7EA");
        public static readonly Guid DriverAuthTokenId = Guid.Parse("AA8B3F57-6C7A-4FC2-9D46-25E8C95A111B");
        public static readonly Guid StoreClerkAuthTokenId = Guid.Parse("5F8A3141-CD38-4618-93B6-2DE63D6D677D");
        public static readonly Guid AdminAuthTokenId = Guid.Parse("7C9D91ED-8C1B-4903-815B-AE906FC9053F");
    }

    // Pre-defined roles
    public static Role CustomerRole => new Role {
        Id = Ids.CustomerRoleId,
        Name = "Customer",
    };

    public static Role DriverRole => new Role {
        Id = Ids.DriverRoleId,
        Name = "Driver",
    };

    public static Role StoreClerkRole => new Role {
        Id = Ids.StoreClerkRoleId,
        Name = "StoreClerk",
    };

    public static Role AdminRole => new Role {
        Id = Ids.AdminRoleId,
        Name = "Admin",
    };

    // Pre-defined users
    public static User CustomerUser => new User {
        Id = Ids.CustomerUserId,
        RoleId = Ids.CustomerRoleId,
        Email = "customer@example.com",
        EmailVerified = true,  // Changed from EmailConfirmed
        PhoneNumber = "+15551234567",
        // PhoneNumberConfirmed property removed as it doesn't exist
        FullName = "Customer User",  // Changed from FirstName/LastName to FullName
        CreatedAt = DateTime.UtcNow.AddDays(-30),
        // UpdatedAt property removed as it doesn't exist
        IsActive = true  // Added this since it exists in User but wasn't included
    };

    public static User DriverUser => new User {
        Id = Ids.DriverUserId,
        RoleId = Ids.DriverRoleId,
        Email = "driver@example.com",
        EmailVerified = true,
        PhoneNumber = "+15557654321",
        FullName = "Driver User",
        CreatedAt = DateTime.UtcNow.AddDays(-20),
        IsActive = true
    };

    public static User AdminUser => new User {
        Id = Ids.AdminUserId,
        RoleId = Ids.AdminRoleId,
        Email = "admin@example.com",
        EmailVerified = true,
        PhoneNumber = "+15559876543",
        FullName = "Admin User",
        CreatedAt = DateTime.UtcNow.AddDays(-60),
        IsActive = true
    };

    public static User StoreClerkUser => new User {
        Id = Ids.StoreClerkUserId,
        RoleId = Ids.StoreClerkRoleId,
        Email = "clerk@example.com",
        EmailVerified = true,
        PhoneNumber = "+15553332222",
        FullName = "Store Clerk User",
        CreatedAt = DateTime.UtcNow.AddDays(-45),
        IsActive = true
    };

    public static User UnverifiedUser => new User {
        Id = Ids.UnverifiedUserId,
        RoleId = Ids.CustomerRoleId,
        Email = "unverified@example.com",
        EmailVerified = false,
        PhoneNumber = "+15551112222",
        FullName = "Unverified User",
        CreatedAt = DateTime.UtcNow.AddDays(-5),
        IsActive = true
    };

    public static User InactiveUser => new User {
        Id = Ids.InactiveUserId,
        RoleId = Ids.CustomerRoleId,
        Email = "inactive@example.com",
        EmailVerified = true,
        PhoneNumber = "+15559998888",
        FullName = "Inactive User",
        CreatedAt = DateTime.UtcNow.AddDays(-90),
        IsActive = false
    };

    /// <summary>
    /// Returns a collection of all predefined test users
    /// </summary>
    /// <returns>List of User objects for testing</returns>
    public static List<User> GetAllTestUsers() => new() {
        CustomerUser,
        DriverUser,
        AdminUser,
        StoreClerkUser,
        UnverifiedUser,
        InactiveUser
    };

    // Auth token generation helper
    public static AuthToken GetAuthTokenForUser(Guid userId) {
        if (userId == Ids.CustomerUserId)
            return new AuthToken {
                Id = Ids.CustomerAuthTokenId,
                UserId = userId,
                AccessToken = "customer-test-token-value",
                RefreshToken = "customer-refresh-token",
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IssuedAt = DateTime.UtcNow.AddDays(-1),
                Role = "Customer"
            };
        else if (userId == Ids.DriverUserId)
            return new AuthToken {
                Id = Ids.DriverAuthTokenId,
                UserId = userId,
                AccessToken = "driver-test-token-value",
                RefreshToken = "driver-refresh-token",
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IssuedAt = DateTime.UtcNow.AddDays(-1),
                Role = "Driver"
            };
        else if (userId == Ids.AdminUserId)
            return new AuthToken {
                Id = Ids.AdminAuthTokenId,
                UserId = userId,
                AccessToken = "admin-test-token-value",
                RefreshToken = "admin-refresh-token",
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IssuedAt = DateTime.UtcNow.AddDays(-1),
                Role = "Admin"
            };

        // Default token if no specific match
        return new AuthToken {
            Id = Guid.NewGuid(),
            UserId = userId,
            AccessToken = $"generic-test-token-{userId}",
            RefreshToken = $"generic-refresh-token-{userId}",
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IssuedAt = DateTime.UtcNow,
            Role = null
        };
    }
}