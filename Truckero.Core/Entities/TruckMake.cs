using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace Truckero.Core.Entities;

public class TruckMake
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public ICollection<TruckModel> Models { get; set; } = new List<TruckModel>();
    public string? Icon { get; set; }
}
