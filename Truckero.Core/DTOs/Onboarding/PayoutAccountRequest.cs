using System;

namespace Truckero.Core.DTOs.Onboarding
{
    public class PayoutAccountRequest
    {
        public Guid PaymentMethodTypeId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public Guid? BankId { get; set; }
        public string? AccountNumberLast4 { get; set; }
        public string? RoutingNumber { get; set; }
        public string? MobileNumber { get; set; }
        public string? PayPalEmail { get; set; }
        public bool IsDefault { get; set; }
        public string? MetadataJson { get; set; }
    }
}