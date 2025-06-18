using System;
using System.ComponentModel.DataAnnotations;

namespace Truckero.Core.DTOs.Billing;

public class PaymentMethodDto
{
    public Guid Id { get; set; } // Used for updates, ignored for adds

    [Required]
    public Guid PaymentMethodTypeId { get; set; }

    [Required]
    public string TokenizedId { get; set; } = string.Empty; // e.g., Stripe token, PayPal nonce

    [StringLength(4)]
    public string? Last4 { get; set; }

    public bool IsDefault { get; set; }

    public string? MetadataJson { get; set; } // For card brand, expiry, etc.

    // Not mapped directly to entity, but useful for client-side display
    public string? PaymentMethodTypeName { get; set; }
    public string? DisplayName { get; set; } // e.g., "Visa ending in 1234"
}