using Truckero.Core.Entities;

namespace Truckero.Core.Interfaces.Repositories;

/// <summary>
/// Interface to be used by client devices that want to call our User API Controller
/// </summary>
public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdAsync(Guid id);
    Task AddAsync(User user);
    Task SaveChangesAsync();
    Task GetByEmailAsync(object email);
}