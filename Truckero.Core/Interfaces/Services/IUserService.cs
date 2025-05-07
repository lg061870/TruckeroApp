using Truckero.Core.Entities;

namespace Truckero.Core.Interfaces.Services;

public interface IUserService
{
    Task<User?> GetByIdAsync(Guid userId);
    Task<User?> GetByEmailAsync(string email);
}
