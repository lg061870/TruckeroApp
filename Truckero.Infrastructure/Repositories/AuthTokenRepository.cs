using Truckero.Core.Entities;
using Truckero.Core.Interfaces;
using Truckero.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Truckero.Core.DTOs.Auth;
using Truckero.Core.Constants; // <-- Added for exception codes

namespace Truckero.Infrastructure.Repositories;

public class AuthTokenRepository : IAuthTokenRepository
{
    private readonly AppDbContext _dbContext;

    public AuthTokenRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AuthToken?> GetByAccessTokenByAccessTokenKeyAsync(string accessToken)
    {
        return await _dbContext.AuthTokens
            .Include(t => t.User) // Needed to check EmailVerified
            .FirstOrDefaultAsync(t => t.AccessToken == accessToken);
    }

    public async Task<AuthToken?> GetByRefreshTokenByRefreshTokenKeyAsync(string refreshToken)
    {
        return await _dbContext.AuthTokens
            .FirstOrDefaultAsync(t => t.RefreshToken == refreshToken);
    }

    public async Task UpdateTokenAsync(AuthToken token)
    {
        try
        {
            _dbContext.AuthTokens.Update(token);
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            // Log and handle gracefully
            throw new UnauthorizedAccessException("Token update failed: token may have been revoked or already used.");
        }
    }

    public async Task<AuthToken?> GetByTokenByUserIdAsync(Guid userId)
    {
        Console.WriteLine($"[Repo] Querying AuthToken for UserId: {userId}");
        var token = await _dbContext.AuthTokens.FirstOrDefaultAsync(t => t.UserId == userId);
        Console.WriteLine($"[Repo] Found Token? {token != null}");
        return token;
    }

    public async Task AddTokenAsync(AuthToken token)
    {
        Console.WriteLine($"[Repo] Adding AuthToken for UserId: {token.UserId}");
        await _dbContext.AuthTokens.AddAsync(token);
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteTokenAsync(AuthToken token)
    {
        _dbContext.AuthTokens.Remove(token);
        await _dbContext.SaveChangesAsync();
    }
    // 🛡️ Utility: Revoke by refresh token (shortcut)
    public async Task RevokeRefreshTokenAsync(string refreshToken)
    {
        var token = await GetByRefreshTokenByRefreshTokenKeyAsync(refreshToken);
        if (token != null)
        {
            _dbContext.AuthTokens.Remove(token);
            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task<AuthToken?> GetLatestTokenAsync()
    {
        return await _dbContext.AuthTokens
            .OrderByDescending(t => t.IssuedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<TokenValidationResult> ValidateAccessTokenAsync(string accessToken)
    {
        var token = await _dbContext.AuthTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.AccessToken == accessToken);

        if (token == null)
        {
            return new TokenValidationResult
            {
                Valid = false,
                Reason = ExceptionCodes.AccessTokenNotFound
            };
        }

        if (token.ExpiresAt <= DateTime.UtcNow)
        {
            return new TokenValidationResult
            {
                Valid = false,
                Reason = ExceptionCodes.AccessTokenExpired
            };
        }

        if (token.User == null || !token.User.EmailVerified)
        {
            return new TokenValidationResult
            {
                Valid = false,
                Reason = ExceptionCodes.EmailNotVerified
            };
        }

        return new TokenValidationResult
        {
            Valid = true,
            Reason = null
        };
    }
}
