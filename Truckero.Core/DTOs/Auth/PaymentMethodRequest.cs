namespace Truckero.Core.DTOs.Auth;

public class PaymentMethodRequest
{
    public Guid PaymentMethodTypeId { get; set; }
    public string TokenizedId { get; set; } = null!;
    public bool IsDefault { get; set; }

    // 🔧 New
    public string? MetadataJson { get; set; }
}
