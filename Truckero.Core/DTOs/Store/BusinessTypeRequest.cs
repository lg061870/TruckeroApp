using System.ComponentModel.DataAnnotations;

namespace Truckero.Core.DTOs;

public class BusinessTypeRequest
{
    public Guid Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(250)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;
}
