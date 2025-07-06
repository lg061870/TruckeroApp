using System.ComponentModel.DataAnnotations;

namespace Truckero.Core.Entities;

public class PaymentMethodType {
    [Key]
    public Guid Id { get; set; }
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    [MaxLength(250)]
    public string? Description { get; set; }
    [MaxLength(2)]
    public string? CountryCode { get; set; }
    public bool IsForPayment { get; set; } = true;
    public bool IsForPayout { get; set; } = false;
    public bool IsActive { get; set; } = true;
    [MaxLength(250)]
    public string? IconUrl { get; set; }

    // --- Remove these from PaymentMethodType! (They belong on PaymentAccount/PayoutAccount) ---
    // public bool IsDefault { get; set; } = false;
    // public bool IsValidated { get; set; } = false;

    // --- Navigation properties ---
    public ICollection<PaymentAccount> PaymentAccounts { get; set; } = new List<PaymentAccount>();
    public ICollection<PayoutAccount> PayoutAccounts { get; set; } = new List<PayoutAccount>();
}