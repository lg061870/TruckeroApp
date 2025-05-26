using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;
using Truckero.Core.DTOs.Auth;
using Truckero.Core.Interfaces;

namespace TruckeroApp.ServiceClients;

public class AuthTokenApiClientService : IAuthTokenRepository
{
    private readonly HttpClient _http;

    public AuthTokenApiClientService(HttpClient httpClient)
    {
        _http = httpClient;
    }

    public async Task<AuthToken?> GetByTokenByUserIdAsync(Guid userId)
        => await _http.GetFromJsonAsync<AuthToken?>($"tokens/user/{userId}");

    public async Task AddTokenAsync(AuthToken token)
        => await _http.PostAsJsonAsync("tokens", token);

    public async Task UpdateTokenAsync(AuthToken token)
        => await _http.PutAsJsonAsync("tokens", token);

    public async Task DeleteTokenAsync(AuthToken token)
        => await _http.SendAsync(new HttpRequestMessage(HttpMethod.Delete, "tokens")
        {
            Content = JsonContent.Create(token)
        });

    public async Task<AuthToken?> GetByRefreshTokenByRefreshTokenKeyAsync(string refreshToken)
        => throw new NotSupportedException("Not exposed via API");

    public async Task<AuthToken?> GetLatestTokenAsync()
        => await _http.GetFromJsonAsync<AuthToken?>("tokens/latest");

    public async Task RevokeRefreshTokenAsync(string refreshToken)
        => await _http.PostAsJsonAsync("tokens/revoke", refreshToken);

    public async Task<TokenValidationResult> ValidateAccessTokenAsync(string token)
    {
        try
        {
            var response = await _http.GetAsync($"tokens/validate?token={Uri.EscapeDataString(token)}");
            if (!response.IsSuccessStatusCode)
                return new TokenValidationResult { Valid = false, Reason = "network_error" };

            var json = await response.Content.ReadAsStringAsync();
            var result = System.Text.Json.JsonSerializer.Deserialize<TokenValidationResult>(json);

            if (result == null)
                return new TokenValidationResult { Valid = false, Reason = "invalid_json" };

            return result;
        }
        catch (Exception ex)
        {
            // Optionally log ex
            return new TokenValidationResult { Valid = false, Reason = "exception" };
        }
    }

    public Task<AuthToken?> GetByAccessTokenByAccessTokenKeyAsync(string accessToken)
    {
        throw new NotSupportedException("Client should use ValidateTokenAsync instead.");
    }
}

