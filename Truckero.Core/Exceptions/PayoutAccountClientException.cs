using System;
using System.Net;

namespace Truckero.Core.Exceptions;

public class PayoutAccountClientException : Exception
{
    public string? Code { get; }
    public HttpStatusCode StatusCode { get; }

    public PayoutAccountClientException(string message, string? code, HttpStatusCode statusCode, Exception? innerException = null)
        : base(message, innerException)
    {
        Code = code;
        StatusCode = statusCode;
    }
}

public class PayoutAccountClientValidationException : Exception
{
    public List<string> ValidationMessages { get; }
    public HttpStatusCode StatusCode { get; }

    public PayoutAccountClientValidationException(string message, List<string> validationMessages, HttpStatusCode statusCode)
        : base(message)
    {
        ValidationMessages = validationMessages;
        StatusCode = statusCode;
    }
}