using TruckeroApp.Interfaces;

namespace TruckeroApp.Services;

public class SecureTokenStorageService : ITokenStorageService
{
    private const string AccessTokenKey = "access_token";
    private const string RefreshTokenKey = "refresh_token";
    private const string TokenExpirationKey = "token_expiration";

    public async Task<string?> GetAccessTokenAsync()
    {
        return await GetAsync(AccessTokenKey);
    }

    public async Task SaveAccessTokenAsync(string token)
    {
        await SetAsync(AccessTokenKey, token);
    }

    public Task ClearAccessTokenAsync()
    {
        SecureStorage.Remove(AccessTokenKey);
        return Task.CompletedTask;
    }

    public async Task<string?> GetRefreshTokenAsync()
    {
        return await GetAsync(RefreshTokenKey);
    }

    public async Task SaveRefreshTokenAsync(string refreshToken)
    {
        await SetAsync(RefreshTokenKey, refreshToken);
    }

    public Task ClearRefreshTokenAsync()
    {
        SecureStorage.Remove(RefreshTokenKey);
        return Task.CompletedTask;
    }

    public async Task SaveTokenExpirationAsync(DateTime expiresAt)
    {
        await SetAsync(TokenExpirationKey, expiresAt.ToString("o")); // ISO 8601 format
    }

    public async Task<DateTime?> GetTokenExpirationAsync()
    {
        var value = await GetAsync(TokenExpirationKey);
        return DateTime.TryParse(value, out var dt) ? dt : null;
    }

    public Task ClearTokenExpirationAsync()
    {
        SecureStorage.Remove(TokenExpirationKey);
        return Task.CompletedTask;
    }

    public async Task ClearAllAsync()
    {
        SecureStorage.Remove(AccessTokenKey);
        SecureStorage.Remove(RefreshTokenKey);
        SecureStorage.Remove(TokenExpirationKey);
        await Task.CompletedTask;
    }

    // --- Internal helpers ---

    private async Task<string?> GetAsync(string key)
    {
        try
        {
            return await SecureStorage.GetAsync(key);
        }
        catch (Exception ex)
        {
            // TODO: Log error
            Console.WriteLine($"[SecureStorage] Error getting key '{key}': {ex.Message}");
            return null;
        }
    }

    private async Task SetAsync(string key, string value)
    {
        try
        {
            await SecureStorage.SetAsync(key, value);
        }
        catch (Exception ex)
        {
            // TODO: Log error
            Console.WriteLine($"[SecureStorage] Error setting key '{key}': {ex.Message}");
        }
    }
}
