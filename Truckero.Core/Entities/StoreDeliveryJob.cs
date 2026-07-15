using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Truckero.Core.Enums;

namespace Truckero.Core.Entities;

public class StoreDeliveryJob {
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid StoreId { get; set; }

    public Store Store { get; set; } = default!;

    [Required]
    public Guid ClerkUserId { get; set; }

    public StoreClerkProfile ClerkProfile { get; set; } = default!;

    public Guid? DriverProfileId { get; set; }

    public DriverProfile? AssignedDriver { get; set; }

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
