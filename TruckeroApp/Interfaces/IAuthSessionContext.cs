using Truckero.Core.DTOs.Auth;

namespace TruckeroApp.Interfaces;

public interface IAuthSessionContext
{
    string? AccessToken { get; }
    string? ActiveRole { get; }
    List<string> AvailableRoles { get; }
    Guid? UserId { get; }
    string? Email { get; }
    bool IsAuthenticated { get; }

    bool IsInitialized { get; } // ✅ ADD THIS

    void Set(string accessToken, List<string> roles, string activeRole);
    void SetFromSession(SessionInfo session, string accessToken);
    void SwitchRole(string role);
    void Clear();
}

