using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Truckero.Core.Entities;

public enum FreightBidStatus {
    Requested = 0,
    BidOpen = 1,
    Accepted = 2,
    Assigned = 3,
    InProgress = 4,
    Completed = 5,
    Cancelled = 6
}

public class FreightBid {
    // --- Identity & Customer ---
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid CustomerProfileId { get; set; } // FK to CustomerProfile

    [ForeignKey(nameof(CustomerProfileId))]
    public virtual CustomerProfile? CustomerProfile { get; set; }

    // --- Pickup Location ---
    [Required, MaxLength(256)]
    public string PickupLocation { get; set; } = string.Empty;
    public double? PickupLat { get; set; }
    public double? PickupLng { get; set; }
    public string? PickupPlusCode { get; set; }

    // --- Delivery Location ---
    [Required, MaxLength(256)]
    public string DeliveryLocation { get; set; } = string.Empty;
    public double? DeliveryLat { get; set; }
    public double? DeliveryLng { get; set; }
    public string? DeliveryPlusCode { get; set; }

    // --- Truck & Job Details ---
    [Required]
    public Guid PreferredTruckTypeId { get; set; }
    [ForeignKey(nameof(PreferredTruckTypeId))]
    public virtual TruckType? PreferredTruckType { get; set; }

    public Guid? TruckCategoryId { get; set; }
    [ForeignKey(nameof(TruckCategoryId))]
    public virtual TruckCategory? TruckCategory { get; set; }

    public Guid? BedTypeId { get; set; }
    [ForeignKey(nameof(BedTypeId))]
    public virtual BedType? BedType { get; set; }

    public Guid? TruckMakeId { get; set; }
    [ForeignKey(nameof(TruckMakeId))]
    public virtual TruckMake? TruckMake { get; set; }

    public Guid? TruckModelId { get; set; }
    [ForeignKey(nameof(TruckModelId))]
    public virtual TruckModel? TruckModel { get; set; }

    [MaxLength(64)]
    public string? Weight { get; set; }
    public string? SpecialInstructions { get; set; }
    public bool Insurance { get; set; }
    public bool TravelWithPayload { get; set; }

    [MaxLength(32)]
    public string? TravelRequirement { get; set; }
    public bool ExpressService { get; set; }

    // --- Payment & Status ---
    public Guid? SelectedPaymentMethodId { get; set; }
    [ForeignKey(nameof(SelectedPaymentMethodId))]
    public virtual PaymentAccount? SelectedPaymentAccount { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public FreightBidStatus Status { get; set; } = FreightBidStatus.Requested;

    // --- Assignment ---
    public Guid? AssignedTruckId { get; set; }
    [ForeignKey(nameof(AssignedTruckId))]
    public virtual Truck? AssignedTruck { get; set; }

    public Guid? AssignedDriverId { get; set; }
    [ForeignKey(nameof(AssignedDriverId))]
    public virtual User? AssignedDriver { get; set; }

    // ========================
    // NAVIGATION PROPERTIES
    // ========================
    public virtual ICollection<FreightBidUseTag> UseTags { get; set; } = new List<FreightBidUseTag>();
    public virtual ICollection<FreightBidHelpOption> HelpOptions { get; set; } = new List<FreightBidHelpOption>();
    public virtual ICollection<DriverBid> DriverBids { get; set; } = new List<DriverBid>();
}
