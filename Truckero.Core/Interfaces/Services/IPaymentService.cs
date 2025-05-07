namespace Truckero.Core.Interfaces.Services;

/// <summary>
/// Interface to be used by client devices that want to call our Payment API (PaymentMethod, PayoutAccount) Controller
/// </summary>
public interface IPaymentService
{
    Task<List<PaymentMethodType>> GetAllPaymentMethods(string countryCode);
    Task<List<PaymentMethodType>> GetAllPayoutMethods(string countryCode);

}

