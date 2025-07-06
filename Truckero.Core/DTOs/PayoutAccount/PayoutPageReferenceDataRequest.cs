using Truckero.Core.DTOs.PaymentMethodType;
using Truckero.Core.DTOs.Shared;

namespace Truckero.Core.DTOs.PayoutAccount; 
public class PayoutAccountReferenceDataRequest {
    public List<PaymentMethodTypeRequest> PayoutMethodTypes { get; set; } = new List<PaymentMethodTypeRequest>();
    public List<BankRequest> Banks { get; set; } = new List<BankRequest>();
    public List<CountryRequest> Countries { get; set; } = new List<CountryRequest>();
}
