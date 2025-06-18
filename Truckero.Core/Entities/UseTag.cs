using System.ComponentModel.DataAnnotations;

namespace Truckero.Core.Entities;

public class UseTag
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    public ICollection<TruckUseTag> TruckUseTags { get; set; } = new List<TruckUseTag>();
}
