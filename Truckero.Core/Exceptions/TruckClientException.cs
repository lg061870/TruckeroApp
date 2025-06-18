using System.Net;

namespace Truckero.Core.Exceptions;

public class TruckClientException : Exception {
    public string? Code { get; }
    public HttpStatusCode StatusCode { get; }

    public TruckClientException(string message, string? code, HttpStatusCode statusCode)
        : base(message) {
        Code = code;
        StatusCode = statusCode;
    }
}
