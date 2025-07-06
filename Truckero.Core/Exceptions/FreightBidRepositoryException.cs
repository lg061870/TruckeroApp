using System;

namespace Truckero.Core.Exceptions; 
public class FreightBidException : Exception {
    public string ErrorCode { get; }
    public FreightBidException(string message, string errorCode, Exception? inner = null)
        : base(message, inner) {
        ErrorCode = errorCode;
    }
}