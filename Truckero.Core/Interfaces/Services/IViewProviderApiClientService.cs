using Truckero.Core.DTOs.CustomerFlow;
using Truckero.Core.DTOs.PaymentAccount; // <-- Add this
using Truckero.Core.DTOs.PayoutAccount;

namespace Truckero.Core.Interfaces.Services {
    public interface IViewProviderApiClientService {
        Task<PayoutAccountReferenceDataRequest?> GetPayoutPageReferenceDataAsync(string? countryCode = "CR");
        Task<PaymentAccountReferenceData?> GetPaymentPageReferenceDataAsync(string? countryCode = "CR"); // <-- Add this
        Task<FreightBidReferenceData?> GetFreightBidReferenceDataAsync();
    }
}
