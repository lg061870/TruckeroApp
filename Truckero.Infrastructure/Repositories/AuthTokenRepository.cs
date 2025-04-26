using Truckero.Core.Entities;
using Truckero.Core.Interfaces;
using Truckero.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Truckero.Infrastructure.Repositories
{
    public class AuthTokenRepository : IAuthTokenRepository
    {
        private readonly AppDbContext _dbContext;

        public AuthTokenRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<AuthToken?> GetByRefreshTokenAsync(string refreshToken)
        {
            return await _dbContext.AuthTokens
                .FirstOrDefaultAsync(t => t.RefreshToken == refreshToken);
        }

        public async Task<AuthToken?> GetByUserIdAsync(Guid userId)
        {
            return await _dbContext.AuthTokens
                .FirstOrDefaultAsync(t => t.UserId == userId);
        }

        public async Task AddAsync(AuthToken token)
        {
            await _dbContext.AuthTokens.AddAsync(token);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateAsync(AuthToken token)
        {
            _dbContext.AuthTokens.Update(token);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(AuthToken token)
        {
            _dbContext.AuthTokens.Remove(token);
            await _dbContext.SaveChangesAsync();
        }
        // 🛡️ Utility: Revoke by refresh token (shortcut)
        public async Task RevokeAsync(string refreshToken)
        {
            var token = await GetByRefreshTokenAsync(refreshToken);
            if (token != null)
            {
                _dbContext.AuthTokens.Remove(token);
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}
