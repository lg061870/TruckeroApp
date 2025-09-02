using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Truckero.Core.Entities;

public class Truck {
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    // Owner
    [Required]
    public Guid DriverProfileId { get; set; }

    [ForeignKey(nameof(DriverProfileId))]
    public DriverProfile DriverProfile { get; set; } = null!;

    // Type/Make/Model
    [Required]
    public Guid TruckTypeId { get; set; }
    [ForeignKey(nameof(TruckTypeId))]
    public TruckType TruckType { get; set; } = null!;

    [Required]
    public Guid TruckMakeId { get; set; }
    [ForeignKey(nameof(TruckMakeId))]
    public TruckMake TruckMake { get; set; } = null!;

    [Required]
    public Guid TruckModelId { get; set; }
    [ForeignKey(nameof(TruckModelId))]
    public TruckModel TruckModel { get; set; } = null!;

    // Category/BedType (nullable)
    public Guid? TruckCategoryId { get; set; }
    [ForeignKey(nameof(TruckCategoryId))]
    public TruckCategory? TruckCategory { get; set; }

    public Guid? BedTypeId { get; set; }
    [ForeignKey(nameof(BedTypeId))]
    public BedType? BedTypeNav { get; set; }

    // Tags (many-to-many)
    public ICollection<TruckUseTag> UseTags { get; set; } = new List<TruckUseTag>();

    // Required fields
    [Required]
    public string LicensePlate { get; set; } = string.Empty;

    [Required]
    public int Year { get; set; }

    // Photos (optional)
    public string? PhotoFrontUrl { get; set; }
    public string? PhotoBackUrl { get; set; }
    public string? PhotoLeftUrl { get; set; }
    public string? PhotoRightUrl { get; set; }

    // Ownership (enum, not a navigation)
    public OwnershipType? OwnershipType { get; set; }

    // Insurance
    [Required]
    public string InsuranceProvider { get; set; } = string.Empty;

    [Required]
    public string PolicyNumber { get; set; } = string.Empty;

    public string? InsuranceDocumentUrl { get; set; }

    // Status flags
    public bool IsVerified { get; set; } = false;
    public bool IsActive { get; set; } = false;
}
