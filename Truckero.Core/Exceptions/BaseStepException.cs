using System;

namespace Truckero.Core.Exceptions;

/// <summary>
/// Base class for custom exceptions that include a StepCode.
/// </summary>
public abstract class BaseStepException : Exception
{
    /// <summary>
    /// A code identifying the specific step or type of error.
    /// </summary>
    public string StepCode { get; }
    public string ErrorCode => StepCode;

    protected BaseStepException(string message, string stepCode)
        : base(message)
    {
        StepCode = stepCode;
    }

    protected BaseStepException(string message, string stepCode, Exception? innerException)
        : base(message, innerException)
    {
        StepCode = stepCode;
    }
}