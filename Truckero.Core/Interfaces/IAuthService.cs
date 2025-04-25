using Truckero.Core.DTOs.Auth;

namespace Truckero.Core.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterUserRequest request);
    Task<AuthResponse> LoginAsync(AuthLoginRequest request);
    Task LogoutAsync(Guid userId);

    // 🔐 New: Exchange valid credentials for JWT + refresh token
    Task<AuthResponse> ExchangeTokenAsync(TokenRequest request);

    // 🔁 New: Validate refresh token and issue new JWT
    Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request);

    // 🔒 New: Send reset link/token to user's email
    Task RequestPasswordResetAsync(PasswordResetRequest request);

    // 🔐 New: Validate token and update password
    Task ConfirmPasswordResetAsync(PasswordResetConfirmRequest request);
}
