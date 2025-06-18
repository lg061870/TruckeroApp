using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Truckero.Core.Entities;

namespace Truckero.Core.Interfaces.Repositories;

public interface IPayoutAccountRepository
{
    Task<PayoutAccount?> GetPayoutAccountByIdAsync(Guid payoutAccountId);
    Task<List<PayoutAccount>> GetPayoutAccountsByUserIdAsync(Guid userId);
    Task<PayoutAccount?> GetDefaultPayoutAccountByUserIdAsync(Guid userId);
    Task AddPayoutAccountAsync(PayoutAccount payoutAccount);
    Task UpdatePayoutAccountAsync(PayoutAccount payoutAccount);
    Task DeletePayoutAccountAsync(Guid payoutAccountId);
    // Alias for legacy usage in OnboardingService
    Task AddAsync(PayoutAccount payoutAccount) => AddPayoutAccountAsync(payoutAccount);
}
