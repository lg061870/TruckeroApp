using Truckero.Core.Entities;
using Truckero.Core.Interfaces;
using System.Collections.Concurrent;
using Truckero.Core.DTOs.Auth;

namespace TruckeroApp.ServiceClients.Mock;

public class AuthTokenMockApiClientService : IAuthTokenRepository
{
    private static readonly ConcurrentDictionary<Guid, AuthToken> _mockStore = new();
    private static AuthToken? _latest;

    public Task<AuthToken?> GetTokenByUserIdAsync(Guid userId)
        => Task.FromResult(_mockStore.TryGetValue(userId, out var token) ? token : null);

    public Task AddTokenAsync(AuthToken token)
    {
        _mockStore[token.UserId] = token;
        _latest = token;
        return Task.CompletedTask;
    }

    public Task UpdateTokenAsync(AuthToken token)
    {
        _mockStore[token.UserId] = token;
        _latest = token;
        return Task.CompletedTask;
    }

    public Task DeleteTokenAsync(AuthToken token)
    {
        _mockStore.TryRemove(token.UserId, out _);
        if (_latest?.UserId == token.UserId) _latest = null;
        return Task.CompletedTask;
    }

    public Task<AuthToken?> GetByRefreshTokenByRefreshTokenKeyAsync(string refreshToken)
    {
        var token = _mockStore.Values.FirstOrDefault(t => t.RefreshToken == refreshToken);
        return Task.FromResult(token);
    }

    public Task<AuthToken?> GetLatestTokenAsync()
        => Task.FromResult(_latest);

    public Task RevokeRefreshTokenAsync(string refreshToken)
    {
        var toRemove = _mockStore.FirstOrDefault(kvp => kvp.Value.RefreshToken == refreshToken);
        if (toRemove.Key != Guid.Empty)
        {
            _mockStore.TryRemove(toRemove.Key, out _);
            if (_latest?.RefreshToken == refreshToken) _latest = null;
        }
        return Task.CompletedTask;
    }

    public Task<AuthToken?> GetAccessTokenByAccessTokenKeyAsync(string accessToken)
    {
        throw new NotImplementedException();
    }

    public Task<TokenValidationResult> ValidateAccessTokenAsync(string accessToken)
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
