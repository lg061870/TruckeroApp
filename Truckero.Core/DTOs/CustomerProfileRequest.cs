// Truckero.Core.DTOs.Onboarding.CustomerProfileRequest.cs
namespace Truckero.Core.DTOs;

public class CustomerProfileRequest
{
    public string FullName { get; set; } = null!;
    public string Address { get; set; } = null!;
    // Optionally: public string PaymentMethodId { get; set; }
}
