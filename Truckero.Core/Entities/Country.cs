using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Truckero.Core.Entities;

public class Country
{
    [Key]
    [MaxLength(2)]
    public string Code { get; set; } = string.Empty; // ISO 3166-1 alpha-2
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    // Optional: Currency, PhonePrefix, etc.
    public ICollection<Bank> Banks { get; set; } = new List<Bank>();
}
