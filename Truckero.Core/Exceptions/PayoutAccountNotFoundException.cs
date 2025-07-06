using Truckero.Core.Constants;

namespace Truckero.Core.Exceptions; 
public class PayoutAccountNotFoundException : BaseStepException {
    public PayoutAccountNotFoundException(string message, string stepCode = ExceptionCodes.PayoutAccountErrorCodes.NotFound, Exception? innerException = null)
        : base(message, stepCode, innerException) { }
}
