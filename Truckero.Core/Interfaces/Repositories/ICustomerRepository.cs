using Truckero.Core.Entities;

namespace Truckero.Core.Interfaces.Repositories;

public interface ICustomerRepository
{
    Task<CustomerProfile?> GetByUserIdAsync(Guid userId);
    Task AddAsync(CustomerProfile profile);
    Task SaveChangesAsync();
}