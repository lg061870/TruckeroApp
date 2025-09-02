
using Truckero.Core.Entities;

namespace Truckero.Core.DTOs.Auth;

public class AuthResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public Guid UserId { get; set; }
    public DateTime ExpiresIn { get; set; }
    public string? Role { get; set; }
    public User LoggedUser { get; set; }
}
