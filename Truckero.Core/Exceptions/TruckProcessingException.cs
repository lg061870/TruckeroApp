using System;

namespace Truckero.Core.Exceptions;

public class TruckProcessingException : BaseStepException
{
    public TruckProcessingException(string message, string stepCode)
        : base(message, stepCode)
    {
    }
    public TruckProcessingException(string message, string stepCode, Exception inner)
        : base(message, stepCode, inner)
    {
    }
}

public class PayoutProcessingException : BaseStepException
{
    public PayoutProcessingException(string message, string stepCode)
        : base(message, stepCode)
    {
    }
    public PayoutProcessingException(string message, string stepCode, Exception inner)
        : base(message, stepCode, inner)
    {
    }
}
