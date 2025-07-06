using System;
using System.Runtime.CompilerServices;

namespace Truckero.Core.DTOs.PayoutAccount;

public class PayoutAccountRequest
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid PaymentMethodTypeId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public Guid? BankId { get; set; }
    public string? AccountNumberLast4 { get; set; }
    public string? RoutingNumber { get; set; }
    public string? MobileNumber { get; set; }
    public string? PayPalEmail { get; set; }
    public bool IsDefault { get; set; }
    public bool IsValidated { get; set; } = false;
    public string? MetadataJson { get; set; }
    public string? PayoutAccountNickName { get; set; }
    public DateTime CreatedAt { get; set; }
}