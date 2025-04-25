using Truckero.Core.Entities;

public class PayoutAccount
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid PaymentMethodTypeId { get; set; } // FK to PaymentMethodType

    public string AccountNumber { get; set; } = null!;
    public string? BankName { get; set; }
    public bool IsDefault { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
    public PaymentMethodType PaymentMethodType { get; set; } = null!;
}

