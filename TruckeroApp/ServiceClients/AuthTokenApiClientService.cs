using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Truckero.Core.Constants;
using Truckero.Core.DTOs.Auth;
using Truckero.Core.Entities;
using Truckero.Core.Interfaces;
using TruckeroApp.Interfaces;
using TruckeroApp.ServiceClients.ApiHelpers;

namespace TruckeroApp.ServiceClients;

/// <summary>
/// API client for AuthTokenRepository endpoints (all require [Authorize]).
/// </summary>
public class AuthTokenApiClientService : IAuthTokenRepository
{
    private readonly HttpClient _http;
    private readonly IAuthSessionContext _session;

    public AuthTokenApiClientService(HttpClient http, IAuthSessionContext session)
    {
        _http = http;
        _session = session;
    }

    private string RequireAccessToken()
        => _session.AccessToken ?? throw new UnauthorizedAccessException("No access token present in session.");

    public async Task<AuthToken?> GetTokenByUserIdAsync(Guid userId)
    {
        var envelope = AuthenticatedEnvelope.Create(RequireAccessToken(), _http, HttpMethod.Get, $"tokens/user/{userId}");
        var response = await envelope.SendAsync<HttpResponseMessage>();
        if (!response.IsSuccessStatusCode)
            return null;
        return await response.Content.ReadFromJsonAsync<AuthToken>();
    }

    public async Task AddTokenAsync(AuthToken token)
    {
        var envelope = AuthenticatedEnvelope.Create(RequireAccessToken(), _http, HttpMethod.Post, "tokens", token);
        var response = await envelope.SendAsync<HttpResponseMessage>();
        response.EnsureSuccessStatusCode();
    }

    public async Task UpdateTokenAsync(AuthToken token)
    {
        var envelope = AuthenticatedEnvelope.Create(RequireAccessToken(), _http, HttpMethod.Put, "tokens", token);
        var response = await envelope.SendAsync<HttpResponseMessage>();
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteTokenAsync(AuthToken token)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, "tokens")
        {
            Content = JsonContent.Create(token)
        };
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", RequireAccessToken());
        var response = await _http.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }

    public async Task<AuthToken?> GetLatestTokenAsync()
    {
        var envelope = AuthenticatedEnvelope.Create(RequireAccessToken(), _http, HttpMethod.Get, "tokens/latest");
        var response = await envelope.SendAsync<HttpResponseMessage>();
        if (!response.IsSuccessStatusCode)
            return null;
        return await response.Content.ReadFromJsonAsync<AuthToken>();
    }

    public async Task RevokeRefreshTokenAsync(string refreshToken)
    {
        var envelope = AuthenticatedEnvelope.Create(RequireAccessToken(), _http, HttpMethod.Post, "tokens/revoke", refreshToken);
        var response = await envelope.SendAsync<HttpResponseMessage>();
        response.EnsureSuccessStatusCode();
    }

    public async Task<TokenValidationResult> ValidateAccessTokenAsync(string token)
    {
        try
        {
            var envelope = AuthenticatedEnvelope.Create(
                token, 
                _http, HttpMethod.Get, 
                $"tokens/validate?token={Uri.EscapeDataString(token)}");

            var response = await envelope.SendAsync<HttpResponseMessage>();

            if (!response.IsSuccessStatusCode)
                return new TokenValidationResult { Valid = false, Reason = "network_error" };

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<TokenValidationResult>(json);

            if (result == null)
                return new TokenValidationResult { Valid = false, Reason = "invalid_json" };

            return result;
        }
        catch (AuthTokenException authEx)
        {
            // Use the message as the error code (trim, validate, or map if necessary)
            var errorCode = authEx.Reason?.Trim() ?? "unauthorized";
            // Optionally, map unknowns or do more sophisticated parsing if you ever need it
            if (string.IsNullOrWhiteSpace(errorCode))
                errorCode = "unknown";
            return new TokenValidationResult { Valid = false, Reason = errorCode };
        }
        catch (UnauthorizedAccessException ax)
        {
            // Use the message as the error code (trim, validate, or map if necessary)
            var errorCode = ax.Message.Trim() ?? "unauthorized";

            // Optionally, map unknowns or do more sophisticated parsing if you ever need it
            if (string.IsNullOrWhiteSpace(errorCode))
                errorCode = "unknown";

            return new TokenValidationResult { Valid = false, Reason = errorCode };
        }
        catch (HttpRequestException httpEx)
        {
            return new TokenValidationResult
            {
                Valid = false,
                Reason = ExceptionCodes.NetworkError,
                ErrorMessage = httpEx.Message
            };
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    // ----- NOT SUPPORTED VIA API -----
    public Task<AuthToken?> GetByRefreshTokenByRefreshTokenKeyAsync(string refreshToken)
        => throw new NotSupportedException("Not exposed via API.");

    public Task<AuthToken?> GetAccessTokenByAccessTokenKeyAsync(string accessToken)
        => throw new NotSupportedException("Client should use ValidateTokenAsync instead.");

    public Task DeleteAllTokensForUserAsync(Guid userId)
        => throw new NotImplementedException("Endpoint not implemented on server.");

    public Task RevokeTokensByUserIdAsync(Guid userId)
    {
        throw new NotImplementedException();
    }
}
