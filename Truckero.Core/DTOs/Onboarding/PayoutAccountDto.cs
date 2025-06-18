using System;
using System.ComponentModel.DataAnnotations;

namespace Truckero.Core.DTOs.Onboarding;

public class PayoutAccountDto
{
    public Guid Id { get; set; } // Used for updates, ignored for adds

    [Required]
    public Guid PaymentMethodTypeId { get; set; }

    [Required]
    [StringLength(4, MinimumLength = 4)]
    public string AccountNumberLast4 { get; set; } = string.Empty;

    public bool IsDefault { get; set; }

    public string? MetadataJson { get; set; } // For any extra details like bank name, branch, etc.

    // Not mapped directly to entity, but useful for client-side display
    public string? PaymentMethodTypeName { get; set; }
}
