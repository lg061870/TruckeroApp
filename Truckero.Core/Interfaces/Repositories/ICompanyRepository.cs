using Truckero.Core.Entities;

namespace Truckero.Core.Interfaces.Repositories;

public interface ICompanyRepository
{
    Task<Company?> GetByIdAsync(Guid id);
    Task AddAsync(Company company);
    Task UpdateAsync(Company company);
    Task SaveChangesAsync();

    Task<List<BusinessType>> GetBusinessTypesAsync();
    Task<List<Industry>> GetIndustriesAsync();
}
