using Microsoft.EntityFrameworkCore;
using Truckero.Core.Entities;
using Truckero.Core.Interfaces.Repositories;
using Truckero.Infrastructure.Data;

namespace Truckero.Infrastructure.Repositories;

public class StoreRepository : IStoreRepository
{
    private readonly AppDbContext _context;

    public StoreRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> CompanyExistsAsync(Guid companyId)
    {
        return await _context.Companies.AnyAsync(c => c.Id == companyId);
    }

    public async Task AddAsync(Store store)
    {
        await _context.Stores.AddAsync(store);
    }

    public async Task<Store?> GetByIdAsync(Guid id)
    {
        return await _context.Stores.FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<List<Store>> GetByCompanyIdAsync(Guid companyId)
    {
        return await _context.Stores
            .AsNoTracking()
            .Where(s => s.CompanyId == companyId)
            .ToListAsync();
    }

    public Task RemoveAsync(Store store)
    {
        _context.Stores.Remove(store);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
