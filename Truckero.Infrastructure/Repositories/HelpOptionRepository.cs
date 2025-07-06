using Microsoft.EntityFrameworkCore;
using Truckero.Core.Entities;
using Truckero.Core.Interfaces.Repositories;
using Truckero.Infrastructure.Data;

namespace Truckero.Infrastructure.Repositories; 
public class HelpOptionRepository : IHelpOptionRepository {
    private readonly AppDbContext _context;
    public HelpOptionRepository(AppDbContext context) {
        _context = context;
    }

    public async Task<List<HelpOption>> GetAllAsync() {
        return await _context.HelpOptions.ToListAsync();
    }

    public async Task<HelpOption?> GetByIdAsync(Guid id) {
        return await _context.HelpOptions.FindAsync(id);
    }

    // Add more CRUD methods as needed
}
