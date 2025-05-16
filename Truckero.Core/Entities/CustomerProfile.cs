using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Truckero.Core.Entities;

public class CustomerProfile
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;

    [Required]
    public string FullName { get; set; } = null!;

    [Required]
    public string Address { get; set; } = null!;

    public string? PhoneNumber { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<PaymentMethod> PaymentMethods { get; set; } = new List<PaymentMethod>();
}
