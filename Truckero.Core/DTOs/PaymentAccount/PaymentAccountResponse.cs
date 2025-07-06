namespace Truckero.Core.DTOs.PaymentAccount;

/// <summary>
/// API response for Payment Account operations
/// </summary>
public class PaymentAccountResponse {
    public bool Success { get; set; }
    public string? Message { get; set; }
    public List<PaymentAccountRequest> PaymentAccounts { get; set; } = new();
    public string? ErrorCode { get; set; }
}
