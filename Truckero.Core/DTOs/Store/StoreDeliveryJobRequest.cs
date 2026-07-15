using System.ComponentModel.DataAnnotations;
using Truckero.Core.Entities;

namespace Truckero.Core.DTOs;

public class StoreDeliveryJobRequest {
    public Guid Id { get; set; }

    [Required]
    public Guid StoreId { get; set; }

    public StoreRequest? Store { get; set; }

    [Required]
    public Guid ClerkUserId { get; set; }

    public StoreClerkProfileRequest? ClerkProfile { get; set; }

    public Guid? DriverProfileId { get; set; }

    // For now we keep only the driver id; a DriverProfileRequest DTO can be added later
    // public DriverProfileRequest? AssignedDriver { get; set; }

    [Required]
    [MaxLength(250)]
    public string DeliveryAddress { get; set; } = default!;

    public string? Notes { get; set; }

    [MaxLength(100)]
    public string? CustomerName { get; set; }

    [MaxLength(50)]
    public string? CustomerPhone { get; set; }

    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

    public DateTime? PreferredDeliveryTime { get; set; }

    public DeliveryJobStatus Status { get; set; } = DeliveryJobStatus.Pending;

    public bool RequiresAssembly { get; set; }

    public bool RequiresHelpers { get; set; }

    public decimal? EstimatedWeightKg { get; set; }

    public decimal? EstimatedVolumeM3 { get; set; }
}
