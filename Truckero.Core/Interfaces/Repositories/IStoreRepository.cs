using Truckero.Core.Entities;

namespace Truckero.Core.Interfaces.Repositories;

public interface IStoreRepository
{
    Task<bool> CompanyExistsAsync(Guid companyId);

    Task AddAsync(Store store);
    Task<Store?> GetByIdAsync(Guid id);
    Task<List<Store>> GetByCompanyIdAsync(Guid companyId);
    Task RemoveAsync(Store store);
    Task SaveChangesAsync();
}
