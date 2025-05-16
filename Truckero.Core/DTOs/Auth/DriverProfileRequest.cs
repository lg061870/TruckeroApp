using System.ComponentModel.DataAnnotations;

public class DriverProfileRequest
{
    [Required]
    public string FullName { get; set; } = null!;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Required]
    public string PhoneNumber { get; set; } = null!;

    [Required]
    public string LicenseNumber { get; set; } = null!;

    [Required]
    [MinLength(8)]
    public string Password { get; set; } = null!;

    [Required]
    public string UserId { get; set; } = null!;

    [Required]
    public string LicenseFrontUrl { get; set; } = null!;

    [Required]
    public string LicenseBackUrl { get; set; } = null!;

    public List<TruckDto> Trucks { get; set; } = new();

    public class TruckDto
    {
        [Required]
        public string LicensePlate { get; set; } = null!;

        [Required]
        public string Make { get; set; } = null!;

        [Required]
        public string Model { get; set; } = null!;

        [Required]
        public string Year { get; set; } = null!;
    }
}
