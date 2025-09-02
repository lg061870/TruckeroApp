using Truckero.Core.Entities;

namespace Truckero.Core.Interfaces.Repositories;

public interface ICustomerProfileRepository
{
    Task<CustomerProfile?> GetCustomerProfileByUserIdAsync(Guid userId);
    Task<IEnumerable<CustomerProfile>> GetAllCustomerProfilesAsync();
    Task AddCustomerProfileAsync(CustomerProfile profile);
    Task UpdateCustomerProfileAsync(CustomerProfile profile);
    Task SaveCustomerProfileChangesAsync();
    Task DeleteCustomerProfileChangesAsync(Guid userId);
}