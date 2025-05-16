namespace Truckero.Core.DTOs.Auth;

public class SessionInfo
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string ActiveRole { get; set; } = "Guest";
    public List<string> AvailableRoles { get; set; } = new();
    public bool TokenValid { get; set; }
}
