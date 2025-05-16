using System.Net.Http.Json;
using Truckero.Core.Interfaces;

namespace TruckeroApp.ServiceClients;

public class AuthTokenApiClientService : IAuthTokenRepository
{
    private readonly HttpClient _http;

    public AuthTokenApiClientService(HttpClient httpClient)
    {
        _http = httpClient;
    }

    public async Task<AuthToken?> GetByUserIdAsync(Guid userId)
        => await _http.GetFromJsonAsync<AuthToken?>($"tokens/user/{userId}");

    public async Task AddAsync(AuthToken token)
        => await _http.PostAsJsonAsync("tokens", token);

    public async Task UpdateAsync(AuthToken token)
        => await _http.PutAsJsonAsync("tokens", token);

    public async Task DeleteAsync(AuthToken token)
        => await _http.SendAsync(new HttpRequestMessage(HttpMethod.Delete, "tokens")
        {
            Content = JsonContent.Create(token)
        });

    public async Task<AuthToken?> GetByRefreshTokenAsync(string refreshToken)
        => throw new NotSupportedException("Not exposed via API");

    public async Task<AuthToken?> GetLatestAsync()
        => await _http.GetFromJsonAsync<AuthToken?>("tokens/latest");

    public async Task RevokeAsync(string refreshToken)
        => await _http.PostAsJsonAsync("tokens/revoke", refreshToken);
}