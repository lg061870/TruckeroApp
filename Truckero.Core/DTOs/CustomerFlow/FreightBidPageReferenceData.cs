using Truckero.Core.DTOs.PaymentMethodType;
using Truckero.Core.Entities;

namespace Truckero.Core.DTOs.CustomerFlow;

public class FreightBidReferenceData {
    public List<TruckType> TruckTypes { get; set; } = new();
    public List<TruckCategory> TruckCategories { get; set; } = new();
    public List<BedType> BedTypes { get; set; } = new();
    public List<UseTag> UseTags { get; set; } = new();
    public List<PaymentMethodTypeRequest> PaymentMethodTypes { get; set; } = new();
    public List<HelpOption> HelpOptions { get; set; } = new();
}