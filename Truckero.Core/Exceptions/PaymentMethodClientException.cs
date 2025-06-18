using System;
using System.Collections.Generic;
using System.Net;

namespace Truckero.Core.Exceptions;

public class PaymentMethodClientException : Exception
{
    public string? Code { get; }
    public HttpStatusCode StatusCode { get; }

    public PaymentMethodClientException(string message, string? code, HttpStatusCode statusCode, Exception? innerException = null)
        : base(message, innerException)
    {
        Code = code;
        StatusCode = statusCode;
    }
}

public class PaymentMethodClientValidationException : Exception
{
    public List<string> ValidationMessages { get; }
    public HttpStatusCode StatusCode { get; }

    public PaymentMethodClientValidationException(string message, List<string> validationMessages, HttpStatusCode statusCode)
        : base(message)
    {
        ValidationMessages = validationMessages;
        StatusCode = statusCode;
    }
}