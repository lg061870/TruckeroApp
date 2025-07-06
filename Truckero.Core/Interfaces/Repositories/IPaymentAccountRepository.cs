using Truckero.Core.Entities;

namespace Truckero.Core.Interfaces.Repositories; 
/// <summary>
/// Repository for PaymentAccount entities. All mutation methods require SaveChangesAsync to persist.
/// </summary>
public interface IPaymentAccountRepository {
    // --- Queries ---
    Task<List<PaymentAccount>> GetPaymentAccountsByUserIdAsync(Guid userId);
    Task<PaymentAccount?> GetPaymentAccountByIdAsync(Guid paymentAccountId);
    Task<PaymentAccount?> GetDefaultPaymentAccountByUserIdAsync(Guid userId);

    // --- Mutations (do NOT call SaveChanges inside these) ---
    Task AddPaymentAccountAsync(PaymentAccount paymentAccount);
    Task UpdatePaymentAccountAsync(PaymentAccount paymentAccount);
    Task DeletePaymentAccountAsync(Guid paymentAccountId);

    /// <summary>
    /// Sets the specified payment account as default for the user (unsets any others).
    /// </summary>
    Task SetDefaultPaymentAccountAsync(Guid userId, Guid paymentAccountId);

    /// <summary>
    /// Marks the payment account as validated for the user.
    /// </summary>
    Task MarkPaymentAccountValidatedAsync(Guid userId, Guid paymentAccountId);

    /// <summary>
    /// Persists all pending changes.
    /// </summary>
    Task SaveChangesAsync();
}
