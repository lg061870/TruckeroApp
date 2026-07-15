using System.ComponentModel.DataAnnotations;

namespace Truckero.Core.Entities;

public class Company {
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = default!;

    [MaxLength(512)]
    public string? LogoUrl { get; set; }

    public Guid? BusinessTypeId { get; set; }

    public BusinessType? BusinessType { get; set; }

    // Legacy JSON-backed industry names; kept for now during migration
    public List<string> Industries { get; set; } = new();

    // New relational link to Industry via join table
    public ICollection<CompanyIndustry> CompanyIndustries { get; set; } = new List<CompanyIndustry>();

    public ICollection<Store> Stores { get; set; } = new List<Store>();

    // Optional: for future expansion if you want explicit user mapping
    public ICollection<User>? Managers { get; set; }
}
