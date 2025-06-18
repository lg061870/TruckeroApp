using System;

namespace Truckero.Core.Exceptions
{
    /// <summary>
    /// Exception for login step errors (email not verified, login failed, unknown error).
    /// </summary>
    public class LoginStepException : BaseStepException
    {
        public LoginStepException(string message, string stepCode)
            : base(message, stepCode)
        {
        }
        public LoginStepException(string message, string stepCode, Exception inner)
            : base(message, stepCode, inner)
        {
        }
    }
}
