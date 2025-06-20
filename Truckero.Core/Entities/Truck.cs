using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Truckero.Core.Entities;

public class Truck {
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid DriverProfileId { get; set; } = Guid.Empty;

    [Required]
    public Guid TruckTypeId { get; set; } = Guid.Empty;

    [ForeignKey(nameof(TruckTypeId))]
    public TruckType TruckType { get; set; }

    [Required]
    public Guid TruckMakeId { get; set; } = Guid.Empty;

    [ForeignKey(nameof(TruckMakeId))]
    public TruckMake TruckMake { get; set; }

    [Required]
    public Guid TruckModelId { get; set; } = Guid.Empty;

    [ForeignKey(nameof(TruckModelId))]
    public TruckModel TruckModel { get; set; }

    [Required]
    public string? LicensePlate { get; set; } = string.Empty;

    [Required]
    public int Year { get; set; } = 0;

    // Photos
    public string? PhotoFrontUrl { get; set; } = null;
    public string? PhotoBackUrl { get; set; } = null;
    public string? PhotoLeftUrl { get; set; } = null;
    public string? PhotoRightUrl { get; set; } = null;

    // Categorization
    public Guid? TruckCategoryId { get; set; } = null;
    [ForeignKey(nameof(TruckCategoryId))]
    public TruckCategory? TruckCategory { get; set; }

    public Guid? BedTypeId { get; set; } = null;
    [ForeignKey(nameof(BedTypeId))]
    public BedType? BedTypeNav { get; set; }

    // Tags
    public ICollection<TruckUseTag> UseTags { get; set; } = new List<TruckUseTag>();

    // Ownership
    public OwnershipType? OwnershipType { get; set; } = null;

    // Insurance
    [Required]
    public string? InsuranceProvider { get; set; } = string.Empty;
    [Required]
    public string? PolicyNumber { get; set; } = string.Empty;
    public string? InsuranceDocumentUrl { get; set; } = null;

    // Status flags
    public bool IsVerified { get; set; } = false;
    public bool IsActive { get; set; } = false;

    // Navigation
    [ForeignKey(nameof(DriverProfileId))]
    public DriverProfile DriverProfile { get; set; }
}
