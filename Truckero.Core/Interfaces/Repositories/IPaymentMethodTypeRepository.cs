using Truckero.Core.Entities;

namespace Truckero.Core.Interfaces.Repositories;

/// <summary>
/// Repository for PaymentMethodType entities.
/// </summary>
public interface IPaymentMethodTypeRepository {
    // --- Entity-based methods ---
    Task<List<PaymentMethodType>> GetAllPaymentMethodTypesAsync();
    Task<List<PaymentMethodType>> GetPaymentMethodTypesByCountryAsync(string countryCode);
    Task<PaymentMethodType?> GetPaymentMethodTypeByIdAsync(Guid id);

    Task<PaymentMethodType> AddPaymentMethodTypeAsync(PaymentMethodType entity);
    Task UpdatePaymentMethodTypeAsync(PaymentMethodType entity);
    Task InactivatePaymentMethodTypeAsync(Guid id);   // Soft delete
    Task DeletePaymentMethodTypeAsync(Guid id);        // Hard delete
}
