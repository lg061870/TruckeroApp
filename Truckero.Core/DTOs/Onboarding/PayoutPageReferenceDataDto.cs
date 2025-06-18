using System.Collections.Generic;
using Truckero.Core.Entities;

namespace Truckero.Core.DTOs.Onboarding;

public class PayoutPageReferenceDataDto
{
    public List<PaymentMethodType> PayoutMethodTypes { get; set; } = new();
    public List<Bank> Banks { get; set; } = new();
    public List<Country> Countries { get; set; } = new();
}
