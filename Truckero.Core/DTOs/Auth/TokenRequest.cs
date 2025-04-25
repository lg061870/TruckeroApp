using System.ComponentModel.DataAnnotations;

namespace Truckero.Core.DTOs.Auth;

public class TokenRequest
{
    [Required]
    public string Email { get; set; } = null!;

    [Required]
    public string Password { get; set; } = null!;
}
