
using System.ComponentModel.DataAnnotations;
using Truckero.Core.DTOs.Auth;

namespace Truckero.Core.Interfaces;

public interface IAuthTokenRepository
{
    Task<AuthToken?> GetByTokenByUserIdAsync(Guid userId);
    Task<AuthToken?> GetByAccessTokenByAccessTokenKeyAsync(string acessTokenKey); // already there
    Task<AuthToken?> GetByRefreshTokenByRefreshTokenKeyAsync(string refreshTokenKey);
    Task<AuthToken?> GetLatestTokenAsync();
    Task AddTokenAsync(AuthToken token);
    Task UpdateTokenAsync(AuthToken token);
    Task DeleteTokenAsync(AuthToken token);
    Task RevokeRefreshTokenAsync(string refreshToken);

    // 🆕 Add this:
    Task<TokenValidationResult> ValidateAccessTokenAsync(string accessToken);
}

