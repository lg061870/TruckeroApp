using Truckero.Core.DTOs.Auth;
using Truckero.Core.Interfaces;

namespace Truckero.Infrastructure.Services.Auth;

public class AuthService : IAuthService
{
    public Task<AuthResponse> LoginAsync(AuthLoginRequest request)
    {
        // 🧪 Simulate bad credentials for testing
        if (request.Email == "invalid@example.com" && request.Password == "wrongpass")
        {
            throw new UnauthorizedAccessException("Invalid credentials");
        }

        // 🟢 Always return a mock token for other inputs
        return Task.FromResult(new AuthResponse
        {
            AccessToken = "fake.jwt.token",
            RefreshToken = "refresh.token"
        });
    }

    public Task<AuthResponse> RegisterAsync(RegisterUserRequest request)
    {
        // 🆕 TODO: Create user record, possibly trigger verification
        return Task.FromResult(new AuthResponse
        {
            AccessToken = "fake.jwt.token",
            RefreshToken = "refresh.token"
        });
    }

    public Task LogoutAsync(Guid userId)
    {
        // 🔓 TODO: Revoke refresh token or invalidate session
        return Task.CompletedTask;
    }

    public Task<AuthResponse> ExchangeTokenAsync(TokenRequest request)
    {
        // 🔄 TODO: Validate grant type / client creds
        return Task.FromResult(new AuthResponse
        {
            AccessToken = "new.jwt.token",
            RefreshToken = "new.refresh.token"
        });
    }

    public Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            throw new ArgumentException("Refresh token is required");
        }

        return Task.FromResult(new AuthResponse
        {
            AccessToken = "refreshed.jwt.token",
            RefreshToken = "refreshed.refresh.token"
        });
    }

    public Task RequestPasswordResetAsync(PasswordResetRequest request)
    {
        // 📧 Send password reset email/token
        return Task.CompletedTask;
    }

    public Task ConfirmPasswordResetAsync(PasswordResetConfirmRequest request)
    {
        // 🔐 Validate token and update password
        return Task.CompletedTask;
    }
}
