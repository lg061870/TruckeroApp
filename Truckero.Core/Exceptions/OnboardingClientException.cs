// 📁 Truckero.Core/Exceptions/OnboardingClientException.cs

using System.Net;

namespace Truckero.Core.Exceptions;

public class OnboardingClientException : HttpRequestException
{
    public string StepCode { get; }

    public OnboardingClientException(string message, string stepCode, HttpStatusCode statusCode)
        : base(message, null, statusCode)
    {
        StepCode = stepCode;
    }
}
