using Truckero.Core.DTOs.Auth;

namespace Truckero.Core.Interfaces.Services;

/// <summary>
/// 🔐 Auth service abstraction for handling identity, session, and role-based flows.
/// </summary>
public interface IAuthService
{
    // 🔐 Core Authentication Lifecycle

    /// <summary>
    /// 📝 Registers a new user and returns a token response if successful.
    /// </summary>
    Task<AuthResponse> RegisterAsync(RegisterUserRequest request);

    /// <summary>
    /// 🔑 Logs in a user using email/password credentials.
    /// </summary>
    Task<AuthResponse> LoginAsync(AuthLoginRequest request);

    /// <summary>
    /// 🚪 Logs out a user and revokes their tokens.
    /// </summary>
    Task LogoutAsync(Guid userId);

    /// <summary>
    /// ♻️ Exchanges a temporary or short-lived token for a new one.
    /// </summary>
    Task<AuthResponse> ExchangeTokenAsync(TokenRequest request);

    /// <summary>
    /// 🔁 Uses a refresh token to acquire a new access token.
    /// </summary>
    Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request);

    /// <summary>
    /// 🛡️ Checks if an access token is still valid server-side.
    /// </summary>
    Task<bool> ValidateTokenAsync(string token);

    // 🔑 Password Recovery

    /// <summary>
    /// 📤 Sends a password reset email/token to the user.
    /// </summary>
    Task RequestPasswordResetAsync(PasswordResetRequest request);

    /// <summary>
    /// 🔐 Confirms password reset with token and new password.
    /// </summary>
    Task ConfirmPasswordResetAsync(PasswordResetConfirmRequest request);

    // 🧭 Role-Based Access

    /// <summary>
    /// 🧭 Gets the active role for the current authenticated session.
    /// </summary>
    Task<AuthToken?> GetLatestAsync();

    /// <summary>
    /// 🧾 Gets all available roles assigned to the current user.
    /// </summary>
    Task<List<string>> GetAllRolesAsync();

    /// <summary>
    /// 🔄 Switches the user's current active role.
    /// </summary>
    Task SetActiveRoleAsync(string role);

    // 📋 Session State Utility

    /// <summary>
    /// 📋 Returns the full session object including roles, identity, and token validity.
    /// </summary>
    Task<SessionInfo> GetSessionAsync();

    /// <summary>
    /// 🎯 Retrieves the currently active role (used for routing, layout, or session setup).
    /// </summary>
    Task<string> GetActiveRoleAsync();

}
