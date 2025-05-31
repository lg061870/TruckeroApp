using Truckero.Core.DTOs.Auth;
using Truckero.Core.Entities;

namespace Truckero.Core.Interfaces.Services;

/// <summary>
/// 🔐 Auth service abstraction for handling identity, session, and role-based flows.
/// </summary>
public interface IAuthService
{
    // 🔐 Authentication Lifecycle
    Task<(User NewUser, AuthToken Token)> RegisterUserAsync(RegisterUserRequest request);
    Task<AuthResponse> LoginUserAsync(AuthLoginRequest request);
    Task LogoutUserAsync(Guid userId);

    // 🔄 Token Management
    Task<AuthResponse> ExchangeTokenAsync(TokenRequest request);
    Task<AuthResponse> RefreshAccessTokenAsync(RefreshTokenRequest request);
    Task<bool> ValidateTokenAsync(string token);

    // 🔑 Password Recovery
    Task RequestPasswordResetAsync(PasswordResetRequest request);
    Task ConfirmPasswordResetAsync(PasswordResetConfirmRequest request);

    // 🧭 Role-Based Access
    Task<AuthToken?> GetLatestAsync();
    Task<List<string>> GetAllRolesAsync();
    Task SetActiveRoleAsync(string role);
    Task<string> GetActiveRoleAsync();

    // 📋 Session State
    Task<SessionInfo> GetSessionAsync();
    Task<User?> GetUserByEmailAsync(string email);
    Task<User?> GetUserByIdAsync(Guid userId);
    Task<User?> GetCurrentUserAsync();
    Task<User?> GetUserByAccessToken(string accessToken);
    Task<AuthResponse> LoginToDeleteAccountAsync(string email, string password);
}

