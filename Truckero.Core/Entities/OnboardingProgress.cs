namespace Truckero.Core.Entities;

public class OnboardingProgress
{
    public Guid UserId { get; set; }
    public string StepCurrent { get; set; } = "start";
    public bool Completed { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
}
