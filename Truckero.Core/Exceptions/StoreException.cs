namespace Truckero.Core.Exceptions;

public class StoreException : Exception
{
    public string Code { get; }

    public StoreException(string code, string message) : base(message)
    {
        Code = code;
    }
}
