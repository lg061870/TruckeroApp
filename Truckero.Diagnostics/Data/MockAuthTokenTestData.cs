using System;
using System.Collections.Generic;
using Truckero.Core.Entities;

namespace Truckero.Diagnostics.Data;

/// <summary>
/// Provides mock authentication token data objects for unit tests
/// </summary>
public static class MockAuthTokenTestData
{
    // Common token identifiers
    public static class Tokens
    {
        // Access tokens
        public static readonly string ValidCustomerAccess = "access_customer_valid_123456";
        public static readonly string ValidDriverAccess = "access_driver_valid_654321";
        public static readonly string ExpiredAccess = "access_expired_987654";
        
        // Refresh tokens
        public static readonly string ValidCustomerRefresh = "refresh_customer_valid_123456";
        public static readonly string ValidDriverRefresh = "refresh_driver_valid_654321";
        public static readonly string ExpiredRefresh = "refresh_expired_987654";
    }
    
    // Token for customer user
    public static AuthToken ValidCustomerToken => new()
    {
        UserId = MockUserTestData.Ids.CustomerUserId,
        AccessToken = Tokens.ValidCustomerAccess,
        RefreshToken = Tokens.ValidCustomerRefresh,
        IssuedAt = DateTime.UtcNow.AddHours(-1),
        ExpiresAt = DateTime.UtcNow.AddDays(7),
        RevokedAt = null
    };
    
    // Token for driver user
    public static AuthToken ValidDriverToken => new()
    {
        UserId = MockUserTestData.Ids.DriverUserId,
        AccessToken = Tokens.ValidDriverAccess,
        RefreshToken = Tokens.ValidDriverRefresh,
        IssuedAt = DateTime.UtcNow.AddHours(-2),
        ExpiresAt = DateTime.UtcNow.AddDays(7),
        RevokedAt = null
    };
    
    // Expired token
    public static AuthToken ExpiredToken => new()
    {
        UserId = MockUserTestData.Ids.CustomerUserId,
        AccessToken = Tokens.ExpiredAccess,
        RefreshToken = Tokens.ExpiredRefresh,
        IssuedAt = DateTime.UtcNow.AddDays(-14),
        ExpiresAt = DateTime.UtcNow.AddDays(-7),
        RevokedAt = null
    };
    
    // Helper to get all test tokens
    public static List<AuthToken> GetAllTestTokens() => new()
    {
        ValidCustomerToken,
        ValidDriverToken,
        ExpiredToken
    };
}