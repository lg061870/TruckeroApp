using Truckero.Core.DTOs.PaymentAccount;

namespace Truckero.Core.Interfaces.Services {
    /// <summary>
    /// Service for business logic and DTO conversion for Payment Accounts.
    /// </summary>
    public interface IPaymentAccountService {
        // --- Payment Account Management (per user/customer) ---

        /// <summary>
        /// Gets all payment accounts for a customer.
        /// </summary>
        Task<PaymentAccountResponse> GetPaymentAccountsByUserIdAsync(Guid userId);

        /// <summary>
        /// Gets a specific payment account by ID.
        /// </summary>
        Task<PaymentAccountResponse> GetPaymentAccountByIdAsync(Guid paymentAccountId);

        /// <summary>
        /// Adds a new payment account for the customer.
        /// </summary>
        Task<PaymentAccountResponse> AddPaymentAccountAsync(PaymentAccountRequest request);

        /// <summary>
        /// Updates an existing payment account.
        /// </summary>
        Task<PaymentAccountResponse> UpdatePaymentAccountAsync(PaymentAccountRequest request);

        /// <summary>
        /// Deletes a payment account by ID.
        /// </summary>
        Task<PaymentAccountResponse> DeletePaymentAccountAsync(Guid userId, Guid paymentAccountId);

        /// <summary>
        /// Sets a payment account as default for the customer.
        /// </summary>
        Task<PaymentAccountResponse> SetDefaultPaymentAccountAsync(Guid userId, Guid paymentAccountId);

        /// <summary>
        /// Marks a payment account as validated.
        /// </summary>
        Task<PaymentAccountResponse> MarkPaymentAccountValidatedAsync(Guid userId, Guid paymentAccountId);
    }
}
