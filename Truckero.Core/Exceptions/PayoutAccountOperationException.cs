namespace Truckero.Core.Exceptions; 
public class PayoutAccountOperationException : BaseStepException {
    public PayoutAccountOperationException(string message, string stepCode, Exception? innerException = null)
        : base(message, stepCode, innerException) { }
}
