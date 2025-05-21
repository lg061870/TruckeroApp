
namespace TruckeroApp.Interfaces;

public interface ITokenStorageService
{
    Task SaveAccessTokenAsync(string token);
    Task<string?> GetAccessTokenAsync();
    Task ClearAccessTokenAsync();

    Task SaveRefreshTokenAsync(string refreshToken);
    Task<string?> GetRefreshTokenAsync();
    Task ClearRefreshTokenAsync();

    Task SaveTokenExpirationAsync(DateTime expiresAt);
    Task<DateTime?> GetTokenExpirationAsync();
    Task ClearTokenExpirationAsync();

    Task ClearAllAsync();

    // ✅ New generic methods
    Task SaveValueAsync(string key, string value);
    Task<string?> GetValueAsync(string key);
    Task RemoveValueAsync(string key);
}