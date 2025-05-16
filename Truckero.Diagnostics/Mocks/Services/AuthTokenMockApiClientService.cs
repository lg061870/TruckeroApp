using Truckero.Core.Entities;
using Truckero.Core.Interfaces;
using System.Collections.Concurrent;

namespace TruckeroApp.ServiceClients.Mock;

public class AuthTokenMockApiClientService : IAuthTokenRepository
{
    private static readonly ConcurrentDictionary<Guid, AuthToken> _mockStore = new();
    private static AuthToken? _latest;

    public Task<AuthToken?> GetByUserIdAsync(Guid userId)
        => Task.FromResult(_mockStore.TryGetValue(userId, out var token) ? token : null);

    public Task AddAsync(AuthToken token)
    {
        _mockStore[token.UserId] = token;
        _latest = token;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(AuthToken token)
    {
        _mockStore[token.UserId] = token;
        _latest = token;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(AuthToken token)
    {
        _mockStore.TryRemove(token.UserId, out _);
        if (_latest?.UserId == token.UserId) _latest = null;
        return Task.CompletedTask;
    }

    public Task<AuthToken?> GetByRefreshTokenAsync(string refreshToken)
    {
        var token = _mockStore.Values.FirstOrDefault(t => t.RefreshToken == refreshToken);
        return Task.FromResult(token);
    }

    public Task<AuthToken?> GetLatestAsync()
        => Task.FromResult(_latest);

    public Task RevokeAsync(string refreshToken)
    {
        var toRemove = _mockStore.FirstOrDefault(kvp => kvp.Value.RefreshToken == refreshToken);
        if (toRemove.Key != Guid.Empty)
        {
            _mockStore.TryRemove(toRemove.Key, out _);
            if (_latest?.RefreshToken == refreshToken) _latest = null;
        }
        return Task.CompletedTask;
    }
}
