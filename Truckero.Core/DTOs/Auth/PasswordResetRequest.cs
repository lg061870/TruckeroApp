using System.ComponentModel.DataAnnotations;

namespace Truckero.Core.DTOs.Auth;

public class PasswordResetRequest
{
    [Required]
    public string Email { get; set; } = null!;
}
