using Truckero.Core.Entities;

namespace Truckero.Core.Interfaces.Repositories;

public interface IRoleRepository
{
    Task<List<Role>> GetAllRolesAsync();
    Task<Role?> GetByNameAsync(string name);
    Task AddAsync(Role role);

    /// <summary>
    /// Returns the default role ID (e.g., "Customer") used during registration.
    /// </summary>
    Task<Guid> GetDefaultRoleIdAsync();
}
