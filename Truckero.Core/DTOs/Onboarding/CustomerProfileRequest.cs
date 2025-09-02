using System.ComponentModel.DataAnnotations;
using Truckero.Core.DTOs.PayoutAccount;
using Truckero.Core.Entities;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Truckero.Core.DTOs.Onboarding;

public class CustomerProfileRequest
{
    [Required(ErrorMessage = "The record appears t.")]
    public Guid UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid Id { get; set; }

    public static CustomerProfileRequest ToCustomerProfileRequest(CustomerProfile profile) {
        if (profile == null) throw new ArgumentNullException(nameof(profile));

        return new CustomerProfileRequest {
            UserId = profile.UserId,
            Id = profile.Id,
            CreatedAt = profile.CreatedAt
        };
    }

    public static CustomerProfileRequest ToCustomerProfileRequest(User user) {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        // Truckero SRS: Defensive for nullable profiles
        var profile = user.CustomerProfile;
        if (profile == null)
            throw new InvalidOperationException("CustomerProfile is missing for user ID: " + user.Id);

        // If any CustomerProfile subfield might be nullable, fallback to default or log a warning
        return new CustomerProfileRequest {
            UserId = profile.UserId,                         // Could be profile?.UserId ?? ""
            Id = profile.Id,
            CreatedAt = profile.CreatedAt
        };
    }
}
