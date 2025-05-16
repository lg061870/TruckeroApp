using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Truckero.Core.Entities;

public class StoreClerkProfile
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;

    public string CorporateEmail { get; set; } = null!;
    public bool Verified { get; set; } = false;

    public ICollection<StoreClerkAssignment> StoreAssignments { get; set; } = new List<StoreClerkAssignment>();
}
