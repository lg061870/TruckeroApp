using Truckero.Core.DTOs.Auth;
using Truckero.Core.Interfaces;

namespace Truckero.Infrastructure.Services.Auth;

public class AuthService : IAuthService
{
    public Task<AuthResponse> LoginAsync(AuthLoginRequest request)
    {
        // 🔐 TODO: Validate credentials against B2C or local
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
}
