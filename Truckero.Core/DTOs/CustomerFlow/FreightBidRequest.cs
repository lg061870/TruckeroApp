using System.ComponentModel.DataAnnotations;

namespace Truckero.Core.DTOs.CustomerFlow; 
public class FreightBidRequest {
    [Required]
    public string PickupLocation { get; set; } = string.Empty;

    [Required]
    public string DeliveryLocation { get; set; } = string.Empty;

    // Instead of string VehicleType, use TruckTypeId
    [Required]
    public Guid TruckTypeId { get; set; }

    // (Optional) Let customer specify required truck features:
    public Guid? TruckCategoryId { get; set; }
    public Guid? BedTypeId { get; set; }
    public List<Guid> UseTagIds { get; set; } = new(); // Customer can select special tags like “For Furniture,” “For Construction,” etc.

    // Payload
    [Required]
    public string PayloadType { get; set; } = string.Empty;
    public string? OtherPayload { get; set; }

    public List<string> HelpOptions { get; set; } = new();

    public bool TravelWithPayload { get; set; }
    public string? TravelRequirement { get; set; }

    public bool Insurance { get; set; }
    public string? Weight { get; set; }
    public string? SpecialInstructions { get; set; }
    public string? PaymentMethodId { get; set; }

    public bool ExpressService { get; set; }
}

