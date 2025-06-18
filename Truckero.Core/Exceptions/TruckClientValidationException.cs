using System.Net;

namespace Truckero.Core.Exceptions; 
public class TruckClientValidationException : Exception {
    public List<string> ValidationMessages { get; }
    public HttpStatusCode StatusCode { get; }

    public TruckClientValidationException(string message, List<string> validationMessages, HttpStatusCode statusCode)
        : base(message) {
        ValidationMessages = validationMessages;
        StatusCode = statusCode;
    }
}


