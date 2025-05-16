namespace TruckeroApp.Interfaces;

public interface ITokenStorageService
{
    Task<string?> GetAccessTokenAsync();
    Task SaveAccessTokenAsync(string token);
    Task ClearAccessTokenAsync();

    Task<string?> GetRefreshTokenAsync();
    Task SaveRefreshTokenAsync(string refreshToken);
    Task ClearRefreshTokenAsync();
}
