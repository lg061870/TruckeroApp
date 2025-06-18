namespace Truckero.Core.Exceptions;

public class ReferentialIntegrityException : Exception {
    public string Code { get; }
    public string ErrorCode => Code;
    public object? Context { get; }

    public ReferentialIntegrityException(string message, string code, object? context = null, Exception? inner = null)
        : base(message, inner) {
        Code = code;
        Context = context;
    }
}
