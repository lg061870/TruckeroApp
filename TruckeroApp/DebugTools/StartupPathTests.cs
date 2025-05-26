using Truckero.Core.DTOs.Auth;
using Truckero.Core.Entities;
using Truckero.Core.Enums;
using Truckero.Core.Interfaces;
using Truckero.Core.Interfaces.Services;
using TruckeroApp.Interfaces;

namespace TruckeroApp.DebugTools;

/// <summary>
/// Provides test token paths for simulating startup flows in DEBUG mode.
/// </summary>
public static class StartupTestPaths
{
    /// <summary>
    /// Simulates scenario with no stored token.
    /// </summary>
    public static async Task NoToken(ITokenStorageService tokenStorage)
    {
        await tokenStorage.ClearAccessTokenAsync();
        await tokenStorage.ClearRefreshTokenAsync();
    }

    /// <summary>
    /// Simulates an invalid token stored locally.
    /// </summary>
    public static async Task InvalidToken(ITokenStorageService tokenStorage)
    {
        const string fakeInvalidToken = "eyFake.Invalid.Payload";
        await tokenStorage.ClearAccessTokenAsync();
        await tokenStorage.ClearRefreshTokenAsync();
        await tokenStorage.SaveAccessTokenAsync(fakeInvalidToken);
    }

    public static Task ValidTokenCustomer(ITokenStorageService ts, IAuthService auth, IAuthTokenRepository repo)
    => ValidToken(ts, auth, repo, RoleType.Customer);

    public static Task ValidTokenDriver(ITokenStorageService ts, IAuthService auth, IAuthTokenRepository repo)
        => ValidToken(ts, auth, repo, RoleType.Driver);

    public static Task ValidTokenStoreClerk(ITokenStorageService ts, IAuthService auth, IAuthTokenRepository repo)
        => ValidToken(ts, auth, repo, RoleType.StoreClerk);

    /// <summary>
    /// Simulates a fully valid token scenario.
    /// </summary>
    public static async Task ValidToken(
        ITokenStorageService tokenStorage,
        IAuthService authService,
        IAuthTokenRepository tokenRepo,
        RoleType role)
    {
        const string mockAccessToken =
            "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9." +
            "eyJzdWIiOiIxMjM0NTY3ODkwIiwiZW1haWwiOiJ0ZXN0QHRydWNrZXJvLmFwcCIsInVzZXJJZCI6IjE4YjFiODc0LWJhYjUtNDQ5ZC04ZmYwLTI1MTc1OGU5NjIxYiIsImV4cCI6MTk1NzUyODAwMH0." +
            "MOCK_SIGNATURE";

        const string mockRefreshToken = "mock-valid-refresh-token";

        // Clear and set local token storage
        await tokenStorage.ClearAccessTokenAsync();
        await tokenStorage.ClearRefreshTokenAsync();
        await tokenStorage.SaveAccessTokenAsync(mockAccessToken);
        await tokenStorage.SaveRefreshTokenAsync(mockRefreshToken);

        // Use the embedded userId from the mock token's payload
        var userId = Guid.Parse("18b1b874-bab5-449d-8ff0-251758e9621b");

        var tokenEntry = new AuthToken
        {
            UserId = userId,
            AccessToken = mockAccessToken,
            RefreshToken = mockRefreshToken,
            Role = role.ToString(),
            IssuedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(1)
        };

        await tokenRepo.AddTokenAsync(tokenEntry);

#if DEBUG
        var confirm = await tokenRepo.GetByTokenByUserIdAsync(userId);
        Console.WriteLine($"[TEST] ValidToken created. Role persisted for user: {confirm?.Role}");
#endif
    }

    /// <summary>
    /// Simulates expired token with no refresh token.
    /// </summary>
    public static async Task TokenWithNoRefreshToken(ITokenStorageService tokenStorage)
    {
        const string expiredToken = "eyFake.Invalid.Payload";
        await tokenStorage.ClearAccessTokenAsync();
        await tokenStorage.ClearRefreshTokenAsync();
        await tokenStorage.SaveAccessTokenAsync(expiredToken);
    }

    /// <summary>
    /// Simulates expired token with a bad refresh token (causes refresh flow to fail).
    /// </summary>
    public static async Task TokenWithBadRefreshToken(ITokenStorageService tokenStorage)
    {
        const string expiredToken = "eyFake.Invalid.Payload";
        await tokenStorage.ClearAccessTokenAsync();
        await tokenStorage.ClearRefreshTokenAsync();
        await tokenStorage.SaveAccessTokenAsync(expiredToken);
        await tokenStorage.SaveRefreshTokenAsync("bad.refresh.token");
    }

    /// <summary>
    /// Simulates expired token with refresh that causes backend exception.
    /// </summary>
    public static async Task TokenWithExceptionOnRefresh(ITokenStorageService tokenStorage)
    {
        const string expiredToken = "eyFake.Invalid.Payload";
        await tokenStorage.ClearAccessTokenAsync();
        await tokenStorage.ClearRefreshTokenAsync();
        await tokenStorage.SaveAccessTokenAsync(expiredToken);
        await tokenStorage.SaveRefreshTokenAsync("throw-exception-token");
    }
}
