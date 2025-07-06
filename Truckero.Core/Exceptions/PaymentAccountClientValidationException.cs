using System;
using System.Collections.Generic;
using System.Net;

namespace Truckero.Core.Exceptions; 
public class PaymentAccountClientValidationException : Exception {
    public List<string> ValidationMessages { get; }
    public HttpStatusCode StatusCode { get; }

    public PaymentAccountClientValidationException(
        string message,
        List<string> validationMessages,
        HttpStatusCode statusCode = HttpStatusCode.BadRequest,
        Exception? innerException = null)
        : base(message, innerException) {
        ValidationMessages = validationMessages ?? new List<string>();
        StatusCode = statusCode;
    }
}
