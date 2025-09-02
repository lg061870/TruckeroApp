using Truckero.Core.DTOs.PaymentMethodType;

public interface IPaymentMethodTypeService {
    Task<PaymentMethodTypeResponse> GetAllPaymentMethodTypesAsync();
    Task<PaymentMethodTypeResponse> GetPaymentMethodTypesByCountryAsync(string countryCode);
    Task<PaymentMethodTypeResponse?> GetPaymentMethodTypeByIdAsync(Guid id);

    Task<PaymentMethodTypeResponse> AddPaymentMethodTypeAsync(PaymentMethodTypeRequest dto);
    Task<PaymentMethodTypeResponse> UpdatePaymentMethodTypeAsync(PaymentMethodTypeRequest dto);
    Task<PaymentMethodTypeResponse> DeletePaymentMethodTypeAsync(Guid id);
}
