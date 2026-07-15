using System.ComponentModel.DataAnnotations;

namespace Truckero.Core.DTOs;

public class CompanyRequest {
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = default!;

    [MaxLength(512)]
    public string? LogoUrl { get; set; }

    public Guid? BusinessTypeId { get; set; }

    public List<string> Industries { get; set; } = new();

    // Navigation to child stores expressed via DTOs, not entities
    public ICollection<StoreRequest> Stores { get; set; } = new List<StoreRequest>();

    // Optional: for future expansion if you want explicit user mapping
    // Keep this as DTO-based if you later add a UserRequest DTO
    public ICollection<UserRequest>? Managers { get; set; }
}
