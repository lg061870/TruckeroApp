using System.ComponentModel.DataAnnotations;


namespace Truckero.Core.DTOs;

public class AuthTokenRequest {
    [Key]
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string AccessToken { get; set; } = null!;

    public string RefreshToken { get; set; } = null!;

    public DateTime IssuedAt { get; set; }

    public DateTime ExpiresAt { get; set; }

    public DateTime? RevokedAt { get; set; }

    // ✅ New: Persist test/debug role assignment
    public string? Role { get; set; }

    public UserRequest User { get; set; } = null!;
}
