namespace Truckero.Core.DTOs.Shared;

public class PaymentAccountJSONType {
    public Guid Id { get; set; } = Guid.NewGuid();
    public bool IsSaved { get; set; } = false;
    public string? PaymentMethodType { get; set; } // "Credit Card", "PayPal", etc.
    public Dictionary<string, string> Fields { get; set; } = new();
}
