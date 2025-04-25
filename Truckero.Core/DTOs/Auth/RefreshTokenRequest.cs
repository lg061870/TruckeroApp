using System.ComponentModel.DataAnnotations;

namespace Truckero.Core.DTOs.Auth;

public class RefreshTokenRequest
{
    [Required]
    public string RefreshToken { get; set; } = null!;
}
