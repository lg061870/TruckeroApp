using Truckero.Core.Entities;

namespace Truckero.Core.Interfaces.Repositories;

public interface IStoreClerkRepository
{
    Task<StoreClerkProfile?> GetByUserIdAsync(Guid userId);
    Task AddAsync(StoreClerkProfile clerkProfile);
    Task SaveChangesAsync();
}
