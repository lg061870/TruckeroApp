using System.Text.Json.Serialization;
using Truckero.Core.DTOs.Onboarding;
using Truckero.Core.Entities;

namespace TruckeroApp.Models;

/// <summary>
/// Model for storing driver registration data in local storage during the registration process
/// </summary>
public class DriverRegistrationData
{
    /// <summary>
    /// The user's generated ID (used for tracking throughout registration)
    /// </summary>
    public Guid UserId { get; set; }
    
    /// <summary>
    /// When the data was last saved to storage
    /// </summary>
    public DateTime LastUpdated { get; set; }
    
    /// <summary>
    /// The driver's profile information
    /// </summary>
    public DriverProfileRequest Profile { get; set; } = new();
    
    /// <summary>
    /// Password entered by the user
    /// </summary>
    public string Password { get; set; } = string.Empty;
    
    /// <summary>
    /// License image URLs
    /// </summary>
    public string? LicenseFrontUrl { get; set; }
    public string? LicenseBackUrl { get; set; }
    
    /// <summary>
    /// Trucks registered by the driver
    /// </summary>
    public List<Truck> Trucks { get; set; } = new();

    /// <summary>
    /// Payout accounts registered by the driver
    /// </summary>
    public List<PayoutAccountDto> PayoutAccounts { get; set; } = new();

    /// <summary>
    /// Indicates whether this registration has been submitted to the API
    /// </summary>
    public bool IsRegisteredWithApi { get; set; } = false;

    /// <summary>
    /// Creates storage model from the registration form data
    /// </summary>
    public static DriverRegistrationData FromRegistrationForm(
        Guid userId,
        DriverProfileRequest profile,
        string password,
        string? licenseFrontUrl,
        string? licenseBackUrl, 
        List<Truck> trucks,
        List<PayoutAccountDto> payoutAccounts,
        bool isRegisteredWithApi = false)
    {
        return new DriverRegistrationData
        {
            UserId = userId,
            LastUpdated = DateTime.Now,
            Profile = profile,
            Password = password,
            LicenseFrontUrl = licenseFrontUrl,
            LicenseBackUrl = licenseBackUrl,
            Trucks = trucks,
            PayoutAccounts = payoutAccounts,
            IsRegisteredWithApi = isRegisteredWithApi
        };
    }
}