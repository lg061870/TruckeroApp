using Microsoft.Maui.Storage;
using System.Threading.Tasks;
using TruckeroApp.Interfaces;

public class SecureTokenStorageService : ITokenStorageService
{
    private const string AccessTokenKey = "access_token";
    private const string RefreshTokenKey = "refresh_token";

    public async Task<string?> GetAccessTokenAsync()
    {
        try
        {
            return await SecureStorage.GetAsync(AccessTokenKey);
        }
        catch
        {
            return null;
        }
    }

    public async Task SaveAccessTokenAsync(string token)
    {
        try
        {
            await SecureStorage.SetAsync(AccessTokenKey, token);
        }
        catch
        {
            // log or handle error
        }
    }

    public Task ClearAccessTokenAsync()
    {
        SecureStorage.Remove(AccessTokenKey);
        return Task.CompletedTask;
    }

    public async Task<string?> GetRefreshTokenAsync()
    {
        try
        {
            return await SecureStorage.GetAsync(RefreshTokenKey);
        }
        catch
        {
            return null;
        }
    }

    public async Task SaveRefreshTokenAsync(string refreshToken)
    {
        try
        {
            await SecureStorage.SetAsync(RefreshTokenKey, refreshToken);
        }
        catch
        {
            // log or handle error
        }
    }

    public Task ClearRefreshTokenAsync()
    {
        SecureStorage.Remove(RefreshTokenKey);
        return Task.CompletedTask;
    }

}
