﻿
namespace Truckero.Core.DTOs.Onboarding;

public class OnboardingProgressResponse
{
    public string Role { get; set; } = null!;
    public string Step { get; set; } = null!;
    public bool EmailVerified { get; set; }
    public bool PhoneVerified { get; set; }
    public string StepCurrent { get; set; }
    public bool Completed { get; set; }
    public DateTime LastUpdated { get; set; }
}
