using Truckero.Core.DTOs.PayoutAccount;

namespace Truckero.Core.Interfaces.Services;

/// <summary>
/// Service for managing payout accounts (DTO-based, chatty style).
/// </summary>
public interface IPayoutAccountService {
    Task<PayoutAccountResponse> GetPayoutAccountByIdAsync(Guid payoutAccountId);
    Task<PayoutAccountResponse> GetPayoutAccountsByUserIdAsync(Guid userId);
    Task<PayoutAccountResponse> GetDefaultPayoutAccountByUserIdAsync(Guid userId);

    Task<PayoutAccountResponse> AddPayoutAccountAsync(Guid userId, PayoutAccountRequest payoutAccountRequest);
    Task<PayoutAccountResponse> UpdatePayoutAccountAsync(Guid userId, Guid payoutAccountId, PayoutAccountRequest payoutAccountRequest);
    Task<PayoutAccountResponse> DeletePayoutAccountAsync(Guid userId, Guid payoutAccountId);
    Task SetDefaultPayoutAccountAsync(Guid userId, Guid payoutAccountId);
}
