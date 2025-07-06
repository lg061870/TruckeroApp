using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Truckero.Core.Entities; 
public class Country {
    [Key]
    [MaxLength(2)]
    [Required]
    public string Code { get; set; } = string.Empty; // ISO 3166-1 alpha-2

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    // Optional: ISO 4217 currency code, e.g., "CRC" or "USD"
    [MaxLength(3)]
    public string? CurrencyCode { get; set; }

    // Optional: E.164 phone prefix, e.g., "+506"
    [MaxLength(8)]
    public string? PhonePrefix { get; set; }

    // Optional: Soft delete/inactivation flag (for reference data lifecycle)
    public bool IsActive { get; set; } = true;

    // Navigation: Banks available in this country
    public ICollection<Bank> Banks { get; set; } = new List<Bank>();
}
