using Microsoft.EntityFrameworkCore;
using Truckero.Core.Entities;
using Truckero.Core.Interfaces.Repositories;
using Truckero.Infrastructure.Data;

namespace Truckero.Infrastructure.Repositories;

public class RoleRepository : IRoleRepository
{
    private readonly AppDbContext _context;

    public RoleRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Role>> GetAllRolesAsync()
    {
        return await _context.Roles.ToListAsync();
    }

    public async Task<Role?> GetByNameAsync(string name)
    {
        return await _context.Roles.FirstOrDefaultAsync(r => r.Name == name);
    }

    public async Task AddAsync(Role role)
    {
        _context.Roles.Add(role);
        await _context.SaveChangesAsync();
    }

    public async Task<Guid> GetDefaultRoleIdAsync()
    {
        var defaultRoleName = "Customer"; // or use RoleType.Customer.ToString()
        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == defaultRoleName);

        if (role == null)
            throw new InvalidOperationException($"Default role '{defaultRoleName}' not found.");

        return role.Id;
    }
}
