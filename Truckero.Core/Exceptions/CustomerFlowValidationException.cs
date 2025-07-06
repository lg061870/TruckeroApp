using System.Net;

namespace Truckero.Core.Exceptions;

public class CustomerFlowValidationException : CustomerFlowClientException {
    public IEnumerable<string> Errors { get; }
    public CustomerFlowValidationException(string message, IEnumerable<string> errors, HttpStatusCode status)
        : base(message, "VALIDATION_FAILED", status) => Errors = errors;
}