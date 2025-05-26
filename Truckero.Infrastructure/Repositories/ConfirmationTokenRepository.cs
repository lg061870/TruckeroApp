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

        public async Task AddAsync(ConfirmationToken token)
        {
            await _dbContext.ConfirmationTokens.AddAsync(token);
        }

        public async Task<ConfirmationToken?> GetByTokenAsync(string token)
        {
            return await _dbContext.ConfirmationTokens.FirstOrDefaultAsync(t => t.Token == token);
        }

        public async Task UpdateAsync(ConfirmationToken token)
        {
            _dbContext.ConfirmationTokens.Update(token);
            await Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _dbContext.SaveChangesAsync();
        }
    }
}
