using Truckero.Core.Entities;

namespace Truckero.Core.DTOs.Auth;

public class SessionInfo
{
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string ActiveRole { get; set; } = "Guest";
    public List<string> AvailableRoles { get; set; } = new();
    public bool TokenValid { get; set; }
    public User LoggedUser { get; set; } = new();
}
