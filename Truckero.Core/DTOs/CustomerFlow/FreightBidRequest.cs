using System.ComponentModel.DataAnnotations;
using Truckero.Core.Entities;
using System.Collections.Generic;
using Truckero.Core.DTOs.Onboarding;

namespace Truckero.Core.DTOs.CustomerFlow; 
public class FreightBidRequest {
    public Guid Id { get; set; }

    [Required]
    public CustomerProfileRequest CustomerProfile { get; set; } = null!;

    [Required]
    public string PickupLocation { get; set; } = string.Empty;
    public double? PickupLat { get; set; }
    public double? PickupLng { get; set; }
    public string? PickupPlusCode { get; set; }

    [Required]
    public string DeliveryLocation { get; set; } = string.Empty;
    public double? DeliveryLat { get; set; }
    public double? DeliveryLng { get; set; }
    public string? DeliveryPlusCode { get; set; }

    // Use ENTITIES instead of IDs
    [Required]
    public TruckType TruckType { get; set; } = null!;

    public TruckCategory? TruckCategory { get; set; }
    public BedType? BedType { get; set; }
    public TruckMake? TruckMake { get; set; }
    public TruckModel? TruckModel { get; set; }

    public List<UseTag> UseTags { get; set; } = new();
    public List<HelpOption> HelpOptions { get; set; } = new();

    public bool TravelWithPayload { get; set; }
    public string? TravelRequirement { get; set; }
    public bool Insurance { get; set; }
    public string? Weight { get; set; }
    public string? SpecialInstructions { get; set; }
    public bool ExpressService { get; set; }
    public Entities.PaymentAccount? PaymentAccount { get; set; }
}
