using Microsoft.EntityFrameworkCore;
using Truckero.Core.Entities;
using Truckero.Core.Interfaces.Repositories;
using Truckero.Infrastructure.Data;

namespace Truckero.Infrastructure.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly AppDbContext _context;

    public CustomerRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<CustomerProfile?> GetCustomerProfileByUserIdAsync(Guid userId)
    {
        return await _context.CustomerProfiles
            .Include(cp => cp.PaymentMethods)
            .FirstOrDefaultAsync(cp => cp.UserId == userId);
    }

    public async Task<IEnumerable<CustomerProfile>> GetAllCustomerProfilesAsync()
    {
        return await _context.CustomerProfiles
            .Include(cp => cp.User)
            .ToListAsync();
    }

    public async Task AddCustomerProfileAsync(CustomerProfile profile)
    {
        await _context.CustomerProfiles.AddAsync(profile);
    }

    public async Task UpdateCustomerProfileAsync(CustomerProfile profile)
    {
        _context.Entry(profile).State = EntityState.Modified;
    }

    public async Task SaveCustomerProfileChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task DeleteCustomerProfileChangesAsync(Guid userId)
    {
        var profile = await _context.CustomerProfiles.FirstOrDefaultAsync(cp => cp.UserId == userId);
        if (profile != null)
        {
            _context.CustomerProfiles.Remove(profile);
            await _context.SaveChangesAsync();
        }
    }
}