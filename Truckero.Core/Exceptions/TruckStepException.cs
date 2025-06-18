using System;

namespace Truckero.Core.Exceptions;

public class TruckStepException : BaseStepException
{
    public TruckStepException(string message, string stepCode, Exception? inner = null)
        : base(message, stepCode, inner)
    {
    }
}