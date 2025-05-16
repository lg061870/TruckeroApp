using Microsoft.EntityFrameworkCore;
using Truckero.Core.Entities;
using Truckero.Core.Interfaces.Repositories;
using Truckero.Infrastructure.Data;

namespace Truckero.Infrastructure.Repositories;

public class StoreClerkRepository : IStoreClerkRepository
{
    private readonly AppDbContext _context;

    public StoreClerkRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<StoreClerkProfile?> GetByUserIdAsync(Guid userId)
    {
        return await _context.StoreClerkProfiles
            .Include(c => c.StoreAssignments)
            .FirstOrDefaultAsync(c => c.UserId == userId);
    }

    public async Task AddAsync(StoreClerkProfile clerkProfile)
    {
        await _context.StoreClerkProfiles.AddAsync(clerkProfile);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
