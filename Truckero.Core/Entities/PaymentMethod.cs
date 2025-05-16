using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using Truckero.Core.Entities;

public class PaymentMethod
{
    public Guid Id { get; set; }
    public Guid PaymentMethodTypeId { get; set; }
    public Guid UserId { get; set; }

    public PaymentMethodType PaymentMethodType { get; set; } = null!;
    public User User { get; set; } = null!;

    public string TokenizedId { get; set; } = null!;
    public bool IsDefault { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(4)]
    public string? Last4 { get; set; }

    // 🌐 Type-specific info (e.g. CardBrand, Expiry, PayPal Email, CryptoWallet)
    public string? MetadataJson { get; set; }

    [NotMapped]
    public object? Metadata =>
    PaymentMethodType.Name switch
    {
        "Card" => JsonSerializer.Deserialize<CardMetadata>(MetadataJson ?? ""),
        "Wallet" => JsonSerializer.Deserialize<WalletMetadata>(MetadataJson ?? ""),
        "PayPal" => JsonSerializer.Deserialize<PayPalMetadata>(MetadataJson ?? ""),
        "Bank" => JsonSerializer.Deserialize<BankMetadata>(MetadataJson ?? ""),
        "Cash" => JsonSerializer.Deserialize<CashMetadata>(MetadataJson ?? ""),
        "Crypto" => JsonSerializer.Deserialize<CryptoMetadata>(MetadataJson ?? ""),
        _ => null
    };

    /// <summary>
    /// Deserialize metadata into the given DTO type (e.g. CardMetadata, CryptoMetadata).
    /// </summary>
    public T? GetMetadata<T>() where T : class
    {
        if (string.IsNullOrWhiteSpace(MetadataJson))
            return null;

        try
        {
            return JsonSerializer.Deserialize<T>(MetadataJson);
        }
        catch
        {
            return null; // or optionally throw
        }
    }
}
