namespace Truckero.Core.Entities;

using System.ComponentModel.DataAnnotations;

public class AuthToken
{
    [Key]
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string AccessToken { get; set; } = null!;

    public string RefreshToken { get; set; } = null!;

    public DateTime IssuedAt { get; set; }

    public DateTime ExpiresAt { get; set; }

    // 🔧 Fix: Add navigation property
    public User User { get; set; } = null!;
}

