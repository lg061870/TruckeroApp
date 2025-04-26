

using Truckero.Core.Entities;

namespace Truckero.Core.Interfaces;

public interface IAuthTokenRepository
{
    Task<AuthToken?> GetByRefreshTokenAsync(string refreshToken);
    Task<AuthToken?> GetByUserIdAsync(Guid userId);
    Task AddAsync(AuthToken token);
    Task UpdateAsync(AuthToken token);
    Task DeleteAsync(AuthToken token);
}

