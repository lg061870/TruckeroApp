using Truckero.Core.DTOs.Onboarding;
using Truckero.Core.Entities;

namespace Truckero.Core.Interfaces.Services;

/// <summary>
/// Interface to be used by client devices that want to call our Customer API Controller
/// </summary>
public interface ICustomerProfileService
{
    Task<CustomerProfileResponse> GetByUserByCustomerProfileIdAsync(Guid userId);
    Task<CustomerProfileResponse> CreateCustomerProfileAsync(CustomerProfileRequest request, Guid userId);
}
