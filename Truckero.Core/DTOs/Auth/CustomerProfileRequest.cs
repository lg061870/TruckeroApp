using System.ComponentModel.DataAnnotations;

namespace Truckero.Core.DTOs.Auth;

public class CustomerProfileRequest
{
    [Required(ErrorMessage = "Full name is required")]
    public string FullName { get; set; } = null!;

    [Required(ErrorMessage = "Address is required")]
    public string Address { get; set; } = null!;

    [Required(ErrorMessage = "Phone number is required")]
    [Phone(ErrorMessage = "Invalid phone number")]
    public string PhoneNumber { get; set; } = null!;

    [Required(ErrorMessage = "Email address is required")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address")]
    public string Email { get; set; } = string.Empty;

}

