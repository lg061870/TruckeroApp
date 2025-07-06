using Truckero.Core.DTOs.Onboarding;
using Truckero.Core.Entities;

namespace Truckero.Core.Interfaces.Services;

/// <summary>
/// Interface to be used by client devices that want to call our Customer API Controller
/// </summary>
public interface ICustomerService
{
    Task<CustomerProfile?> GetByUserIdAsync(Guid userId);
    Task CreateAsync(CustomerProfileRequest request, Guid userId);
}
