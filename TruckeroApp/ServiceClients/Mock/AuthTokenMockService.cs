using Truckero.Core.DTOs.Auth;
using Truckero.Core.Entities;
using Truckero.Core.Interfaces;

namespace TruckeroApp.ServiceClients.Mock;

public class AuthTokenMockService : IAuthTokenRepository
{
    public Task<AuthToken?> GetTokenByUserIdAsync(Guid userId)
        => Task.FromResult(AuthMockStore.Tokens.TryGetValue(userId, out var token) ? token : null);

    public Task AddTokenAsync(AuthToken token)
    {
        AuthMockStore.Tokens[token.UserId] = token;
        AuthMockStore.Latest = token;
        return Task.CompletedTask;
    }

    public Task UpdateTokenAsync(AuthToken token)
    {
        AuthMockStore.Tokens[token.UserId] = token;
        AuthMockStore.Latest = token;
        return Task.CompletedTask;
    }

    public Task DeleteTokenAsync(AuthToken token)
    {
        AuthMockStore.Tokens.TryRemove(token.UserId, out _);
        if (AuthMockStore.Latest?.UserId == token.UserId)
            AuthMockStore.Latest = null;
        return Task.CompletedTask;
    }

    public Task<AuthToken?> GetByRefreshTokenByRefreshTokenKeyAsync(string refreshToken)
        => Task.FromResult(AuthMockStore.Tokens.Values.FirstOrDefault(t => t.RefreshToken == refreshToken));

    public Task<AuthToken?> GetLatestTokenAsync()
        => Task.FromResult(AuthMockStore.Latest);

    public Task RevokeRefreshTokenAsync(string refreshToken)
    {
        var match = AuthMockStore.Tokens.FirstOrDefault(kvp => kvp.Value.RefreshToken == refreshToken);
        if (match.Key != Guid.Empty)
        {
            AuthMockStore.Tokens.TryRemove(match.Key, out _);
            if (AuthMockStore.Latest?.RefreshToken == refreshToken)
                AuthMockStore.Latest = null;
        }
        return Task.CompletedTask;
    }

    public Task<AuthToken?> GetAccessTokenByAccessTokenKeyAsync(string accessToken)
    {
        throw new NotImplementedException();
    }

    Task<TokenValidationResult> IAuthTokenRepository.ValidateAccessTokenAsync(string accessToken)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAllTokensForUserAsync(Guid userId)
    {
        throw new NotImplementedException();
    }

    public Task RevokeTokensByUserIdAsync(Guid userId)
    {
        throw new NotImplementedException();
    }
}