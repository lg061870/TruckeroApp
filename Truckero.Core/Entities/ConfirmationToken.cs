using System;

namespace Truckero.Core.Entities;

public class ConfirmationToken
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Token { get; set; } = null!;
    public bool Used { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UsedAt { get; set; }
    public required ConfirmationTokenType Type { get; set; } // e.g., "EmailConfirmation", "PasswordReset"
}

public enum ConfirmationTokenType
{
    EmailConfirmation,
    PasswordReset
}
