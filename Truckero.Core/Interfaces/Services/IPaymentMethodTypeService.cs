using Truckero.Core.DTOs.PaymentMethodType;

public interface IPaymentMethodTypeService {
    Task<List<PaymentMethodTypeRequest>> GetAllPaymentMethodTypesAsync();
    Task<List<PaymentMethodTypeRequest>> GetPaymentMethodTypesByCountryAsync(string countryCode);
    Task<PaymentMethodTypeRequest?> GetPaymentMethodTypeByIdAsync(Guid id);

    Task<PaymentMethodTypeRequest> AddPaymentMethodTypeAsync(PaymentMethodTypeRequest dto);
    Task UpdatePaymentMethodTypeAsync(PaymentMethodTypeRequest dto);
    Task DeletePaymentMethodTypeAsync(Guid id);
}
