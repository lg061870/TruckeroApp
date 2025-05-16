using TruckeroApp.Interfaces;
using Truckero.Core.DTOs.Auth;

namespace TruckeroApp.Services;

public class AuthSessionContextService : IAuthSessionContext
{

    public string? AccessToken { get; private set; }
    public string? ActiveRole { get; private set; }
    public List<string> AvailableRoles { get; private set; } = new();
    public Guid? UserId { get; private set; }
    public string? Email { get; private set; }
    public bool IsAuthenticated => !string.IsNullOrWhiteSpace(AccessToken);
    public bool IsInitialized { get; private set; } // ✅ IMPLEMENTED

    public void Set(string accessToken, List<string> roles, string activeRole)
    {
        AccessToken = accessToken;
        AvailableRoles = roles;
        ActiveRole = activeRole;
        IsInitialized = true;
    }

    public void SetFromSession(SessionInfo session, string accessToken)
    {
        AccessToken = accessToken;
        AvailableRoles = session.AvailableRoles ?? new();
        ActiveRole = session.ActiveRole;
        UserId = session.UserId;
        Email = session.Email;
        IsInitialized = true;
    }

    public void SwitchRole(string role)
    {
        ActiveRole = role;
    }

    public void Clear()
    {
        AccessToken = null;
        AvailableRoles.Clear();
        ActiveRole = null;
        UserId = null;
        Email = null;
        IsInitialized = true; // ✅ Still mark as initialized to avoid fallback
    }
}
