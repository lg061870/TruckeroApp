using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Truckero.Core.Entities;

public class Store
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public string Name { get; set; } = null!;

    public string? LegalEntity { get; set; }

    public string? DomainWhitelist { get; set; }

    // 📌 "HQ", "Branch", "Franchise", etc.
    public string? StoreType { get; set; }

    // 🌲 Self-referencing Parent Store (for hierarchy)
    public Guid? ParentStoreId { get; set; }
    [ForeignKey(nameof(ParentStoreId))]
    public Store? ParentStore { get; set; }
    public ICollection<Store> Branches { get; set; } = new List<Store>();

    // ✅ Authorization flag (e.g., approved by Truckero Admin)
    public bool IsAuthorized { get; set; } = false;

    // 🌍 Location metadata
    public string? Region { get; set; }
    public string? Address { get; set; }

    // 🕓 Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // 🔁 Navigation
    public ICollection<StoreClerkAssignment> Clerks { get; set; } = new List<StoreClerkAssignment>();

}
