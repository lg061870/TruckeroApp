using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace Truckero.Core.Entities {
    public class PaymentAccount {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        public Guid PaymentMethodTypeId { get; set; }

        public PaymentMethodType PaymentMethodType { get; set; } = null!;
        public User User { get; set; } = null!;

        // Display/friendly
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;     // Name on card/account
        [MaxLength(64)]
        public string? PaymentAccountNickName { get; set; }

        // Security: tokenized id for processing
        [MaxLength(128)]
        public string TokenizedId { get; set; } = null!;

        // Optional bank/card/account info
        public Guid? BankId { get; set; }
        [MaxLength(4)]
        public string? AccountNumberLast4 { get; set; }     // For card/bank display
        [MaxLength(32)]
        public string? RoutingNumber { get; set; }
        [MaxLength(32)]
        public string? MobileNumber { get; set; }
        [MaxLength(128)]
        public string? PayPalEmail { get; set; }

        // Status/flags
        public bool IsDefault { get; set; }
        public bool IsValidated { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Flexible metadata (card brand, expiry, wallet, etc)
        public string? MetadataJson { get; set; }

        [NotMapped]
        public object? Metadata => MetadataJson == null ? null :
            PaymentMethodType?.Name switch {
                "Card" => JsonSerializer.Deserialize<CardMetadata>(MetadataJson),
                "Wallet" => JsonSerializer.Deserialize<WalletMetadata>(MetadataJson),
                "PayPal" => JsonSerializer.Deserialize<PayPalMetadata>(MetadataJson),
                "Bank" => JsonSerializer.Deserialize<BankMetadata>(MetadataJson),
                "Cash" => JsonSerializer.Deserialize<CashMetadata>(MetadataJson),
                "Crypto" => JsonSerializer.Deserialize<CryptoMetadata>(MetadataJson),
                _ => null
            };

        // Utility: strongly-typed metadata
        public T? GetMetadata<T>() where T : class {
            if (string.IsNullOrWhiteSpace(MetadataJson)) return null;
            try { return JsonSerializer.Deserialize<T>(MetadataJson); } catch { return null; }
        }
    }
}
