using System.ComponentModel.DataAnnotations;
using Truckero.Core.DataAnnotations;

namespace Truckero.Core.DTOs.Auth;

public class CustomerOnboardingRequest
{
    public Guid UserId { get; set; }  // 👈 Add this line

    [Required(ErrorMessage = "Full name is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Full name must be between 2 and 100 characters")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email address is required")]
    [EmailAddress(ErrorMessage = "Invalid email address format")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Phone number is required")]
    [Phone(ErrorMessage = "Invalid phone number format")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Address is required")]
    [StringLength(200, MinimumLength = 5, ErrorMessage = "Address must be between 5 and 200 characters")]
    public string Address { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters long")]
    [StrongPassword(ErrorMessage = "Password must include uppercase, lowercase, digit, and symbol")]
    public string Password { get; set; } = string.Empty;

    public bool HasPaymentMethod { get; set; }
}