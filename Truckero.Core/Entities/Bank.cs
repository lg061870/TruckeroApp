using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Truckero.Core.Entities;

public class Bank
{
    [Key]
    public Guid Id { get; set; }
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    [Required]
    [MaxLength(11)]
    public string SwiftCode { get; set; } = string.Empty;
    [MaxLength(20)]
    public string? BankCode { get; set; }
    [MaxLength(34)]
    public string? IbanPrefix { get; set; }
    [MaxLength(2)]
    [ForeignKey("Country")]
    public string CountryCode { get; set; } = string.Empty;
    public Country Country { get; set; } = null!;
    // Optional: LogoUrl, etc.
}