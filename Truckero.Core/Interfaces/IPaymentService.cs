namespace Truckero.Core.Interfaces;

public interface IPaymentService
{
    Task<List<PaymentMethodType>> GetAllPaymentMethods(string countryCode);
    Task<List<PaymentMethodType>> GetAllPayoutMethods(string countryCode);

}

