using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Truckero.Core.Entities;

namespace Truckero.Core.Interfaces.Repositories;

/// <summary>
/// Repository for CRUD operations on PayoutAccount entities.
/// </summary>
public interface IPayoutAccountRepository {
    /// <summary>
    /// Gets a single payout account by ID (entity).
    /// </summary>
    Task<PayoutAccount?> GetPayoutAccountByIdAsync(Guid payoutAccountId);

    /// <summary>
    /// Gets all payout accounts for a user (entity).
    /// </summary>
    Task<List<PayoutAccount>> GetPayoutAccountsByUserIdAsync(Guid userId);

    /// <summary>
    /// Gets the default payout account for a user (entity).
    /// </summary>
    Task<PayoutAccount?> GetDefaultPayoutAccountByUserIdAsync(Guid userId);

    /// <summary>
    /// Adds a payout account. Does NOT save changes (call SaveChangesAsync after).
    /// </summary>
    Task AddPayoutAccountAsync(PayoutAccount payoutAccount);

    /// <summary>
    /// Updates a payout account. Does NOT save changes (call SaveChangesAsync after).
    /// </summary>
    Task UpdatePayoutAccountAsync(PayoutAccount payoutAccount);

    /// <summary>
    /// Deletes a payout account by ID. Does NOT save changes (call SaveChangesAsync after).
    /// </summary>
    Task DeletePayoutAccountAsync(Guid payoutAccountId);

    /// <summary>
    /// Saves any pending changes (Unit of Work pattern).
    /// </summary>
    Task SaveChangesAsync();

    // Alias for legacy usage in OnboardingService (optional)
    Task AddAsync(PayoutAccount payoutAccount) => AddPayoutAccountAsync(payoutAccount);
}
