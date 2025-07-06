namespace Truckero.Core.DTOs.PaymentMethodType; 
public class PaymentMethodTypeRequest {
    public Guid Id { get; set; }                    // Use Guid for Id
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? CountryCode { get; set; }        // ISO 3166-1 alpha-2
    public bool IsForPayment { get; set; } = true;
    public bool IsForPayout { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public string? IconUrl { get; set; }
    // Add more fields if you add them to the entity
}
