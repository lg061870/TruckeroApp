using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Truckero.Core.Entities;

public class CustomerProfile
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;

    // FullName, Email, Address and PhoneNumber properties moved to User entity

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<PaymentAccount> PaymentAccounts { get; set; } = new List<PaymentAccount>();

    [NotMapped]
    public bool HasPaymentMethods => PaymentAccounts.Count > 0;

    public virtual ICollection<FreightBid> FreightBids { get; set; } = new List<FreightBid>();

}
