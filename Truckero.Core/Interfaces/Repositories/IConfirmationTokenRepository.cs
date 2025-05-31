using System;
using System.Threading.Tasks;
using Truckero.Core.Entities;

namespace Truckero.Core.Interfaces.Repositories
{
    public interface IConfirmationTokenRepository
    {
        Task AddConfirmationTokenAsync(ConfirmationToken token);
        Task<ConfirmationToken?> GetConfirmationTokenByTokenAndTypeAsync(string token, ConfirmationTokenType type);
        Task UpdateConfirmationTokenAsync(ConfirmationToken token);
        Task SaveConfirmationTokenChangesAsync();
        Task<List<ConfirmationToken>> GetUnusedTokensForUserAsync(Guid userId, ConfirmationTokenType type, Guid excludeTokenId);
        Task UpdateConfirmationTokensAsync(IEnumerable<ConfirmationToken> tokens);

    }
}
