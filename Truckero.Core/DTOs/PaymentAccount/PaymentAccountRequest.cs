using System;

namespace Truckero.Core.DTOs.PaymentAccount;

/// <summary>
/// DTO for creating or updating a Payment Account (what a customer uses to pay).
/// </summary>
public class PaymentAccountRequest {
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid PaymentMethodTypeId { get; set; }

    // For Card/Account naming ("John Doe" on Card, etc)
    public string FullName { get; set; } = string.Empty;

    // For cards/bank/etc - optional by payment type
    public Guid? BankId { get; set; }
    public string? AccountNumberLast4 { get; set; }
    public string? RoutingNumber { get; set; }
    public string? MobileNumber { get; set; }
    public string? PayPalEmail { get; set; }

    // Is this the default payment account for this user?
    public bool IsDefault { get; set; }
    public bool IsValidated { get; set; } = false;

    // Metadata is for type-specific info (Card brand, expiry, crypto wallet, etc.)
    public string? MetadataJson { get; set; }

    // Friendly nickname for display
    public string? PaymentAccountNickName { get; set; }
}
