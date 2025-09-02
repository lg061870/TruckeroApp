using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Truckero.Core.Entities;


namespace Truckero.Core.DTOs;

public class OnboardingProgressRequest {
    [Key]
    public Guid UserId { get; set; }

    public string StepCurrent { get; set; } = "start";
    public bool Completed { get; set; }

    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    public static OnboardingProgressRequest ToOnboardingProgressRequest(OnboardingProgress onboardingProgress) {
        if (onboardingProgress == null)
            throw new ArgumentNullException(nameof(onboardingProgress));

        return new OnboardingProgressRequest {
            UserId = onboardingProgress.UserId,
            StepCurrent = onboardingProgress.StepCurrent ?? "start",
            Completed = onboardingProgress.Completed,
            LastUpdated = onboardingProgress.LastUpdated
        };
    }
}

