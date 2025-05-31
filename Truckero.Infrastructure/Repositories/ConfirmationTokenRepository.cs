using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Truckero.Core.Entities;
using Truckero.Core.Interfaces.Repositories;
using Truckero.Infrastructure.Data;

namespace Truckero.Infrastructure.Repositories
{
    public class ConfirmationTokenRepository : IConfirmationTokenRepository
    {
        private readonly AppDbContext _dbContext;

        public ConfirmationTokenRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddConfirmationTokenAsync(ConfirmationToken token)
        {
            await _dbContext.ConfirmationTokens.AddAsync(token);
        }

        public async Task<ConfirmationToken?> GetConfirmationTokenByTokenAndTypeAsync(string token, ConfirmationTokenType type)
        {
            return await _dbContext.ConfirmationTokens
                .FirstOrDefaultAsync(t => t.Token == token && t.Type == type);
        }

        public async Task UpdateConfirmationTokenAsync(ConfirmationToken token)
        {
            _dbContext.ConfirmationTokens.Update(token);
            await Task.CompletedTask;
        }

        public async Task SaveConfirmationTokenChangesAsync()
        {
            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<ConfirmationToken>> GetUnusedTokensForUserAsync(Guid userId, ConfirmationTokenType type, Guid excludeTokenId)
        {
            return await _dbContext.ConfirmationTokens
                .Where(t => t.UserId == userId &&
                            t.Type == type &&
                            !t.Used &&
                            t.Id != excludeTokenId)
                .ToListAsync();
        }

        public Task UpdateConfirmationTokensAsync(IEnumerable<ConfirmationToken> tokens)
        {
            foreach (var token in tokens)
            {
                _dbContext.ConfirmationTokens.Update(token);
            }

            return Task.CompletedTask;
        }

    }
}
