// PaymentAccountReferenceData.cs
using Truckero.Core.DTOs.PaymentMethodType;
using Truckero.Core.DTOs.Shared;

namespace Truckero.Core.DTOs.PaymentAccount; 
public class PaymentAccountReferenceData {
    public List<PaymentMethodTypeRequest> PaymentMethodTypes { get; set; } = new();
    public List<BankRequest> Banks { get; set; } = new();
    public List<CountryRequest> Countries { get; set; } = new();
}