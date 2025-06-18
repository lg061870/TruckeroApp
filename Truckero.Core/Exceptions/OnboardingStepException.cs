// 📁 Truckero.Core/Exceptions/OnboardingStepException.cs

using System; // Added for Exception type

namespace Truckero.Core.Exceptions;

public class OnboardingStepException : BaseStepException // Changed from Exception
{
    public OnboardingStepException(string message, string stepCode, Exception? inner = null) // Renamed 'step' to 'stepCode' for consistency
        : base(message, stepCode, inner) // Pass stepCode to base constructor
    {
    }
}
