using Truckero.Core.Entities;

namespace Truckero.Core.Interfaces.Repositories;

/// <summary>
/// Interface to be used by client devices that want to call our User API Controller
/// </summary>
public interface IUserRepository
{
    Task<User?> GetUserByEmailAsync(string email);
    Task<User?> GetUserByIdAsync(Guid id);
    Task<User?> GetUserByAccessTokenAsync(string accessToken);
    Task AddUserAsync(User user);
    Task SaveUserChangesAsync();

    Task DeleteUserAsync(User user);
}