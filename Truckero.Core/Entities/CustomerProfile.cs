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

    public ICollection<PaymentMethod> PaymentMethods { get; set; } = new List<PaymentMethod>();

    [NotMapped]
    public bool HasPaymentMethods => PaymentMethods.Count > 0;
}
