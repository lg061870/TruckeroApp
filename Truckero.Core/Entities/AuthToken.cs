using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Truckero.Core.Entities;

public class AuthToken
{
    [Key]
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string AccessToken { get; set; } = null!;

    public string RefreshToken { get; set; } = null!;

    public DateTime IssuedAt { get; set; }

    public DateTime ExpiresAt { get; set; }

    public DateTime? RevokedAt { get; set; } // ✅ Optional but useful

    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;
}
