using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Truckero.Core.Entities;

public class OnboardingProgress
{
    [Key]
    public Guid UserId { get; set; }

    public string StepCurrent { get; set; } = "start";
    public bool Completed { get; set; }

    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;
}
