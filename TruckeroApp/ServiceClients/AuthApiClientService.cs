using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Truckero.Core.DTOs.Auth;
using Truckero.Core.Entities;
using Truckero.Core.Exceptions;
using Truckero.Core.Interfaces.Services;
using TruckeroApp.Interfaces;
using TruckeroApp.ServiceClients.ApiHelpers;

namespace TruckeroApp.ServiceClients;

/// <summary>
/// 🔌 Connects mobile app to the AuthController endpoints (production).
/// </summary>
public class AuthApiClientService : IAuthService
{
    private readonly HttpClient _http;
    private readonly IAuthSessionContext _session;

    public AuthApiClientService(HttpClient http, IAuthSessionContext session)
    {
        _http = http;
        _session = session;
    }

    // --- Unauthenticated methods (no session token needed) ---

    public async Task<AuthResponse> LoginUserAsync(AuthLoginRequest request)
    {
        var response = await _http.PostAsJsonAsync("auth/login", request);
        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            if (response.Content.Headers.ContentType?.MediaType?.Contains("json") == true)
            {
                try
                {
                    var error = JsonSerializer.Deserialize<LoginErrorResponse>(content, _jsonOptions);
                    if (error != null && !string.IsNullOrWhiteSpace(error.StepCode))
                        throw new LoginStepException(error.Message ?? "Login failed", error.StepCode);
                }
                catch (JsonException) { /* fallback below */ }
            }
            // fallback: throw with raw content
            throw new UnauthorizedAccessException($"Login failed: {content}");
        }
        return await response.Content.ReadFromJsonAsync<AuthResponse>() ?? throw new InvalidOperationException("Empty login response");
    }

    private class LoginErrorResponse
    {
        public string? Message { get; set; }
        public string? StepCode { get; set; }
    }

    public async Task<(User NewUser, AuthToken Token)> RegisterUserAsync(RegisterUserRequest request)
    {
        var response = await _http.PostAsJsonAsync("auth/register", request);
        response.EnsureSuccessStatusCode();

        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();

        if (authResponse == null)
            throw new InvalidOperationException("Empty register response");

        var user = new User
        {
            Id = authResponse.UserId,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber ?? "",
            EmailVerified = false,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var token = new AuthToken
        {
            AccessToken = authResponse.AccessToken,
            RefreshToken = authResponse.RefreshToken,
            Role = "Customer",
            IssuedAt = DateTime.UtcNow,
            ExpiresAt = authResponse.ExpiresIn
        };

        return (user, token);
    }

    // --- Authenticated methods (use session token) ---

    private string RequireAccessToken()
        => _session.AccessToken ?? throw new UnauthorizedAccessException("No access token present in session.");

    public async Task LogoutUserAsync(Guid userId)
    {
        var envelope = AuthenticatedEnvelope.Create(RequireAccessToken(), _http, HttpMethod.Post, "auth/logout", userId);
        await envelope.SendAsync();
    }

    public async Task<AuthResponse> ExchangeTokenAsync(TokenRequest request)
    {
        var envelope = AuthenticatedEnvelope.Create(RequireAccessToken(), _http, HttpMethod.Post, "auth/exchange", request);
        var response = await envelope.SendAsync<HttpResponseMessage>();
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<AuthResponse>() ?? throw new InvalidOperationException("Exchange failed");
    }

    public async Task<AuthResponse> RefreshAccessTokenAsync(RefreshTokenRequest request)
    {
        var envelope = AuthenticatedEnvelope.Create(RequireAccessToken(), _http, HttpMethod.Post, "auth/refresh", request);
        var response = await envelope.SendAsync<HttpResponseMessage>();
        if (!response.IsSuccessStatusCode)
            throw new UnauthorizedAccessException("Refresh failed");
        return await response.Content.ReadFromJsonAsync<AuthResponse>() ?? throw new InvalidOperationException("Empty refresh response");
    }

    public async Task<bool> ValidateTokenAsync(string token)
    {
        var endpoint = $"auth/validate?accessToken={token}";
        var envelope = AuthenticatedEnvelope.Create(RequireAccessToken(), _http, HttpMethod.Get, endpoint);
        var response = await envelope.SendAsync<HttpResponseMessage>();
        return response.IsSuccessStatusCode;
    }

    public async Task RequestPasswordResetAsync(PasswordResetRequest request)
    {
        var envelope = AuthenticatedEnvelope.Create(RequireAccessToken(), _http, HttpMethod.Post, "auth/password/request-reset", request);
        var response = await envelope.SendAsync<HttpResponseMessage>();
        response.EnsureSuccessStatusCode();
    }

    public async Task ConfirmPasswordResetAsync(PasswordResetConfirmRequest request)
    {
        var envelope = AuthenticatedEnvelope.Create(RequireAccessToken(), _http, HttpMethod.Post, "auth/password/confirm-reset", request);
        var response = await envelope.SendAsync<HttpResponseMessage>();
        response.EnsureSuccessStatusCode();
    }

    public async Task<AuthToken?> GetLatestAsync()
    {
        var envelope = AuthenticatedEnvelope.Create(RequireAccessToken(), _http, HttpMethod.Get, "auth/token/latest");
        return await envelope.SendAsync<AuthToken>();
    }

    public async Task<List<string>> GetAllRolesAsync()
    {
        var envelope = AuthenticatedEnvelope.Create(RequireAccessToken(), _http, HttpMethod.Get, "auth/role/all");
        return await envelope.SendAsync<List<string>>() ?? new();
    }

    public async Task SetActiveRoleAsync(string role)
    {
        var envelope = AuthenticatedEnvelope.Create(RequireAccessToken(), _http, HttpMethod.Post, "auth/role/set", role);
        var response = await envelope.SendAsync<HttpResponseMessage>();
        response.EnsureSuccessStatusCode();
    }

    public async Task<SessionInfo> GetSessionAsync()
    {
        var envelope = AuthenticatedEnvelope.Create(RequireAccessToken(), _http, HttpMethod.Get, "auth/session");
        return await envelope.SendAsync<SessionInfo>() ?? new SessionInfo();
    }

    public async Task<string> GetActiveRoleAsync()
    {
        var envelope = AuthenticatedEnvelope.Create(RequireAccessToken(), _http, HttpMethod.Get, "auth/role/active");
        var response = await envelope.SendAsync<HttpResponseMessage>();

        var role = await response.Content.ReadFromJsonAsync<string>(); // ✅ Fix: parse JSON string

        return string.IsNullOrWhiteSpace(role) ? "Unknown" : role;
    }


    public async Task<User?> GetUserByEmailAsync(string email)
    {
        var endpoint = $"auth/user/by-email?email={Uri.EscapeDataString(email)}";
        var envelope = AuthenticatedEnvelope.Create(RequireAccessToken(), _http, HttpMethod.Get, endpoint);
        var response = await envelope.SendAsync<HttpResponseMessage>();
        if (!response.IsSuccessStatusCode)
            return null;
        return await response.Content.ReadFromJsonAsync<User>();
    }

    public async Task<User?> GetUserByIdAsync(Guid userId)
    {
        var endpoint = $"auth/user/by-id?userId={userId}";
        var envelope = AuthenticatedEnvelope.Create(RequireAccessToken(), _http, HttpMethod.Get, endpoint);
        return await envelope.SendAsync<User>();
    }

    public async Task<User?> GetCurrentUserAsync()
    {
        try
        {
            var envelope = AuthenticatedEnvelope.Create(RequireAccessToken(), _http, HttpMethod.Get, "auth/user/me");
            return await envelope.SendAsync<User>();
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<AuthResponse> LoginToDeleteAccountAsync(string email, string password)
    {
        var payload = new { email, password };
        var envelope = AuthenticatedEnvelope.Create(RequireAccessToken(), _http, HttpMethod.Post, "/auth/login-delete", payload);
        var response = await envelope.SendAsync<HttpResponseMessage>();
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<AuthResponse>()
               ?? throw new InvalidOperationException("No response from login-delete.");
    }

    public async Task<User?> GetUserByAccessToken(string accessToken)
    {
        var url = $"auth/user/by-access-token?token={Uri.EscapeDataString(accessToken)}";
        var envelope = AuthenticatedEnvelope.Create(RequireAccessToken(), _http, HttpMethod.Get, url);
        var response = await envelope.SendAsync<HttpResponseMessage>();
        if (!response.IsSuccessStatusCode)
            return null;
        return await response.Content.ReadFromJsonAsync<User>();
    }

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
}
