using System;

namespace Truckero.Core.Exceptions;

/// <summary>
/// Represents errors that occur during payout account processing steps.
/// </summary>
public class PayoutAccountStepException : BaseStepException
{
    public PayoutAccountStepException(string message, string stepCode, Exception? innerException = null)
        : base(message, stepCode, innerException)
    {
    }
}