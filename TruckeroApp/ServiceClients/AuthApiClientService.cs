using System.Net;
using System.Net.Http.Json;
using Truckero.Core.DTOs.Auth;
using Truckero.Core.Entities;
using Truckero.Core.Interfaces.Services;

namespace TruckeroApp.ServiceClients;

/// <summary>
/// 🔌 Connects mobile app to the AuthController endpoints (production).
/// </summary>
public class AuthApiClientService : IAuthService
{
    private readonly HttpClient _http;

    public AuthApiClientService(HttpClient http)
    {
        _http = http;
    }

    public async Task<AuthResponse> LoginUserAsync(AuthLoginRequest request)
    {
        var response = await _http.PostAsJsonAsync("auth/login", request);
        if (!response.IsSuccessStatusCode)
            throw new UnauthorizedAccessException("Login failed");

        return await response.Content.ReadFromJsonAsync<AuthResponse>() ?? throw new InvalidOperationException("Empty login response");
    }

    public async Task<(User NewUser, AuthToken Token)> RegisterUserAsync(RegisterUserRequest request)
    {
        var response = await _http.PostAsJsonAsync("auth/register", request);
        response.EnsureSuccessStatusCode();

        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();

        if (authResponse == null)
            throw new InvalidOperationException("Empty register response");

        // You’ll need to map or retrieve the `User` object (if it's not included in the response)
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


    public async Task LogoutUserAsync(Guid userId)
    {
        await _http.PostAsJsonAsync("auth/logout", userId);
    }

    public async Task<AuthResponse> ExchangeTokenAsync(TokenRequest request)
    {
        var response = await _http.PostAsJsonAsync("auth/exchange", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<AuthResponse>() ?? throw new InvalidOperationException("Exchange failed");
    }

    public async Task<AuthResponse> RefreshAccessTokenAsync(RefreshTokenRequest request)
    {
        var response = await _http.PostAsJsonAsync("auth/refresh", request);
        if (!response.IsSuccessStatusCode)
            throw new UnauthorizedAccessException("Refresh failed");

        return await response.Content.ReadFromJsonAsync<AuthResponse>() ?? throw new InvalidOperationException("Empty refresh response");
    }

    public async Task<bool> ValidateTokenAsync(string token)
    {
        var response = await _http.GetAsync($"auth/validate?accessToken={token}");
        return response.IsSuccessStatusCode;
    }

    public async Task RequestPasswordResetAsync(PasswordResetRequest request)
    {
        var response = await _http.PostAsJsonAsync("auth/password/request-reset", request);
        response.EnsureSuccessStatusCode();
    }

    public async Task ConfirmPasswordResetAsync(PasswordResetConfirmRequest request)
    {
        var response = await _http.PostAsJsonAsync("auth/password/confirm-reset", request);
        response.EnsureSuccessStatusCode();
    }

    public async Task<AuthToken?> GetLatestAsync()
    {
        return await _http.GetFromJsonAsync<AuthToken>("auth/token/latest");
    }

    public async Task<List<string>> GetAllRolesAsync()
    {
        return await _http.GetFromJsonAsync<List<string>>("auth/role/all") ?? new();
    }

    public async Task SetActiveRoleAsync(string role)
    {
        var response = await _http.PostAsJsonAsync("auth/role/set", role);
        response.EnsureSuccessStatusCode();
    }

    public async Task<SessionInfo> GetSessionAsync()
    {
        return await _http.GetFromJsonAsync<SessionInfo>("auth/session") ?? new SessionInfo();
    }

    public async Task<string> GetActiveRoleAsync()
    {
        var role = await _http.GetStringAsync("auth/role/active");
        return string.IsNullOrWhiteSpace(role) ? "Unknown" : role;
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        var response = await _http.GetAsync($"auth/user/by-email?email={Uri.EscapeDataString(email)}");

        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<User>();
    }

    public async Task<User?> GetUserByIdAsync(Guid userId)
    {
        return await _http.GetFromJsonAsync<User?>($"auth/user/by-id?userId={userId}");
    }

    public async Task<User?> GetCurrentUserAsync()
    {
        try
        {
            return await _http.GetFromJsonAsync<User?>("auth/user/me");
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }
}
