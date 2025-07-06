using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Truckero.Core.Entities;

namespace Truckero.Core.DTOs.Trucks;

public class TruckRequest
{
    public Guid Id { get; set; }
    [Required]
    public Guid TruckTypeId { get; set; } // Use TruckTypeId for vehicle type
    [Required]
    public Guid TruckMakeId { get; set; }
    [Required]
    public Guid TruckModelId { get; set; } // Add TruckModelId
    [Required]
    public string LicensePlate { get; set; } = string.Empty;
    [Required]
    public int Year { get; set; }
    public string? PhotoFrontUrl { get; set; }
    public string? PhotoBackUrl { get; set; }
    public string? PhotoLeftUrl { get; set; }
    public string? PhotoRightUrl { get; set; }
    public Guid? TruckCategoryId { get; set; }
    public Guid? BedTypeId { get; set; }
    public List<Guid>? UseTagIds { get; set; } = new();
    public OwnershipType? OwnershipType { get; set; }
    public string? InsuranceProvider { get; set; }
    public string? PolicyNumber { get; set; }
    public string? InsuranceDocumentUrl { get; set; }
    public Guid DriverProfileId { get; set; }

    // Status flags
    public bool IsVerified { get; set; } = false;
    public bool IsActive { get; set; } = false;


    // Tags
    public ICollection<Guid> UseTags { get; set; } = new List<Guid>();
    public string? TruckType { get; internal set; }
    public string? TruckMake { get; internal set; }
    public string? TruckModel { get; internal set; }
    public string? TruckCategory { get; internal set; }
    public string? BetType { get; internal set; }
}