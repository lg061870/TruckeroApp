// 📁 Truckero.Core/Exceptions/OnboardingStepException.cs

namespace Truckero.Core.Exceptions;

public class OnboardingStepException : Exception
{
    public string Step { get; }

    public OnboardingStepException(string message, string step, Exception? inner = null)
        : base(message, inner)
    {
        Step = step;
    }
}
