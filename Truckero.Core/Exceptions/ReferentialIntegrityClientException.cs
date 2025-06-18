using System.Net;

namespace Truckero.Core.Exceptions;

public class ReferentialIntegrityClientException : Exception {
    public string ErrorCode { get; }
    public object? Context { get; }
    public HttpStatusCode StatusCode { get; }

    public ReferentialIntegrityClientException(string message, string errorCode, HttpStatusCode statusCode, object? context = null)
        : base(message) {
        ErrorCode = errorCode;
        Context = context;
        StatusCode = statusCode;
    }
}
