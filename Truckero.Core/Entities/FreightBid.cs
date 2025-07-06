using System.ComponentModel.DataAnnotations;
using Truckero.Core.Entities;

public enum FreightBidStatus {
    Requested = 0,  // Customer submitted, not open to drivers yet
    BidOpen = 1,    // Open for drivers to submit bids
    Accepted = 2,   // Customer accepted a driver's bid
    Assigned = 3,   // Truck assigned, job ready to start
    InProgress = 4, // Job in progress
    Completed = 5,  // Job finished
    Cancelled = 6   // Cancelled by customer or admin
}

public class FreightBid {
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid CustomerId { get; set; } // FK to User

    [Required]
    [MaxLength(256)]
    public string PickupLocation { get; set; } = string.Empty;

    [Required]
    [MaxLength(256)]
    public string DeliveryLocation { get; set; } = string.Empty;

    [Required]
    public Guid? PreferredTruckTypeId { get; set; }  // FK to TruckType

    [MaxLength(64)]
    public string? Weight { get; set; }

    public string? SpecialInstructions { get; set; }

    public bool Insurance { get; set; }

    public bool TravelWithPayload { get; set; }

    [MaxLength(32)]
    public string? TravelRequirement { get; set; } // "optional" or "required"

    public bool ExpressService { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Guid? SelectedPaymentMethodId { get; set; }   // For payment (can be null at this stage)

    // --- Workflow status
    [Required]
    public FreightBidStatus Status { get; set; } = FreightBidStatus.Requested;

    // --- Assignment
    public Guid? AssignedTruckId { get; set; }           // FK to Truck (after acceptance)
    public Guid? AssignedDriverId { get; set; }          // FK to User (Driver) if assigned

    // --- Navigation Properties ---
    public ICollection<FreightBidUseTag> UseTags { get; set; } = new List<FreightBidUseTag>();
    public ICollection<DriverBid> DriverBids { get; set; } = new List<DriverBid>();
}
