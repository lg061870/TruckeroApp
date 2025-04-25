using Truckero.Core.Entities;

public class PaymentMethod
{
    public Guid Id { get; set; } // ✅ PRIMARY KEY
    public Guid PaymentMethodTypeId { get; set; } // ✅ FK
    public Guid UserId { get; set; }

    public PaymentMethodType PaymentMethodType { get; set; } = null!;
    public User User { get; set; } = null!;

    public string TokenizedId { get; set; } = null!;
    public bool IsDefault { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
