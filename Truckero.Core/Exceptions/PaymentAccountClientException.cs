using System;
using System.Net;

namespace Truckero.Core.Exceptions; 
public class PaymentAccountClientException : Exception {
    public string? ErrorCode { get; }
    public HttpStatusCode StatusCode { get; }

    public PaymentAccountClientException(
        string message,
        string? errorCode = null,
        HttpStatusCode statusCode = HttpStatusCode.BadRequest,
        Exception? innerException = null)
        : base(message, innerException) {
        ErrorCode = errorCode;
        StatusCode = statusCode;
    }
}
