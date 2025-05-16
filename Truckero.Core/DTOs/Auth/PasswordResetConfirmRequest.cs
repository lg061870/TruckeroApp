using System.ComponentModel.DataAnnotations;

namespace Truckero.Core.DTOs.Auth;

public class PasswordResetConfirmRequest
{
    [Required]
    public string Token { get; set; } = null!;

    [Required]
    [MinLength(6)]
    public string? NewPassword { get; set; } = null!;
    public string? Email { get; set; }
}
