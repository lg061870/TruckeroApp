using System.ComponentModel.DataAnnotations;

namespace Truckero.Core.DTOs.Auth;

public class RegisterUserRequest
{
    public Guid UserId { get; set; }
    [Required]
    public string Email { get; set; } = null!;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = null!;

    [Required]
    public string Role { get; set; } = null!;

    [Required]
    public string Name { get; set; } = null!;
    public string? PaymentMetadataJson { get; set; }
    public string PhoneNumber { get; set; }
}
