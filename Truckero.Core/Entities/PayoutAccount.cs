using Truckero.Core.Entities;

public class PayoutAccount
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid PaymentMethodTypeId { get; set; }

    // Optional — move AccountNumber to Metadata for flexibility
    public string? AccountNumberLast4 { get; set; }

    public bool IsDefault { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
    public PaymentMethodType PaymentMethodType { get; set; } = null!;

    // 🧾 Custom fields (e.g. RoutingNumber, CryptoNetwork)
    public string? MetadataJson { get; set; }
}
