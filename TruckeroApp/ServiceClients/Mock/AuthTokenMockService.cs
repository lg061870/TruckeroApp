using Truckero.Core.Interfaces;

namespace TruckeroApp.ServiceClients.Mock;

public class AuthTokenMockService : IAuthTokenRepository
{
    public Task<AuthToken?> GetByUserIdAsync(Guid userId)
        => Task.FromResult(AuthMockStore.Tokens.TryGetValue(userId, out var token) ? token : null);

    public Task AddAsync(AuthToken token)
    {
        AuthMockStore.Tokens[token.UserId] = token;
        AuthMockStore.Latest = token;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(AuthToken token)
    {
        AuthMockStore.Tokens[token.UserId] = token;
        AuthMockStore.Latest = token;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(AuthToken token)
    {
        AuthMockStore.Tokens.TryRemove(token.UserId, out _);
        if (AuthMockStore.Latest?.UserId == token.UserId)
            AuthMockStore.Latest = null;
        return Task.CompletedTask;
    }

    public Task<AuthToken?> GetByRefreshTokenAsync(string refreshToken)
        => Task.FromResult(AuthMockStore.Tokens.Values.FirstOrDefault(t => t.RefreshToken == refreshToken));

    public Task<AuthToken?> GetLatestAsync()
        => Task.FromResult(AuthMockStore.Latest);

    public Task RevokeAsync(string refreshToken)
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
}