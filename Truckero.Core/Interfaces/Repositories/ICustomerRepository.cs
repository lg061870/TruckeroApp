using Truckero.Core.Entities;

namespace Truckero.Core.Interfaces.Repositories;

public interface ICustomerRepository
{
    Task<CustomerProfile?> GetCustomerProfileByUserIdAsync(Guid userId);
    Task AddCustomerProfileAsync(CustomerProfile profile);
    Task SaveCustomerProfileChangesAsync();
}