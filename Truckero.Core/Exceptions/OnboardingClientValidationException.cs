using System.Net;

namespace Truckero.Core.Exceptions;

public class OnboardingClientValidationException : Exception
{
    public List<string> Errors { get; }
    public HttpStatusCode StatusCode { get; }

    public OnboardingClientValidationException(string message, List<string> errors, HttpStatusCode statusCode)
        : base(message)
    {
        Errors = errors;
        StatusCode = statusCode;
    }
}
