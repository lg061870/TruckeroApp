using System;
using System.Threading.Tasks;
using Truckero.Core.Entities;

namespace Truckero.Core.Interfaces.Repositories
{
    public interface IConfirmationTokenRepository
    {
        Task AddAsync(ConfirmationToken token);
        Task<ConfirmationToken?> GetByTokenAsync(string token);
        Task UpdateAsync(ConfirmationToken token);
        Task SaveChangesAsync();
    }
}
