using System.Net;

namespace Truckero.Core.Exceptions;
public class CustomerFlowClientException : Exception {
    public string? ErrorCode { get; }
    public HttpStatusCode StatusCode { get; }
    public CustomerFlowClientException(string message, string? code, HttpStatusCode status, Exception? inner = null)
        : base(message, inner) { ErrorCode = code; StatusCode = status; }
}
