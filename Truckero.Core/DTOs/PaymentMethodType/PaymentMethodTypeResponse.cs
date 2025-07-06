namespace Truckero.Core.DTOs.PaymentMethodType;

/// <summary>
/// Standard API response for PaymentMethodType operations.
/// </summary>
public class PaymentMethodTypeResponse {
    public bool Success { get; set; }
    public string? Message { get; set; }
    public List<PaymentMethodTypeRequest> PaymentMethodTypes { get; set; } = new();
    public string? ErrorCode { get; set; }
}
