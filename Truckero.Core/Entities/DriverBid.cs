using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class DriverBid {
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid FreightBidId { get; set; }
    [ForeignKey(nameof(FreightBidId))]
    public FreightBid FreightBid { get; set; } = null!;

    [Required]
    public Guid DriverId { get; set; }           // FK to User (Driver)

    public Guid TruckId { get; set; }            // FK to Truck

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal OfferAmount { get; set; }

    public string? Message { get; set; }         // Optional message from driver

    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

    // Status of this bid: Pending, Accepted, Rejected, Withdrawn
    [MaxLength(16)]
    public string Status { get; set; } = "Pending";
}
