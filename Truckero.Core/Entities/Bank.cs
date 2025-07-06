using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Truckero.Core.Entities {
    public class Bank {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(11)]
        public string SwiftCode { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? BankCode { get; set; } // Local/regional code (if used)

        [MaxLength(34)]
        public string? IbanPrefix { get; set; } // For IBAN validation

        [Required]
        [MaxLength(2)]
        public string CountryCode { get; set; } = string.Empty;

        [ForeignKey(nameof(CountryCode))]
        public Country Country { get; set; } = null!;

        // Optional: Logo, display icon, or website for richer UI
        [MaxLength(250)]
        public string? LogoUrl { get; set; }

        // Optional: Inactivation flag for safe reference-data updates
        public bool IsActive { get; set; } = true;
    }
}
