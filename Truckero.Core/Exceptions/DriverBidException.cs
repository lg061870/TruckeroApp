using System;

namespace Truckero.Core.Exceptions; 
public class DriverBidException : Exception {
    public string Code { get; }

    public DriverBidException(string message, string code)
        : base(message) {
        Code = code;
    }

    public DriverBidException(string message, string code, Exception inner)
        : base(message, inner) {
        Code = code;
    }
}
