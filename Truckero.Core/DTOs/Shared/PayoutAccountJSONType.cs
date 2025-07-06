namespace Truckero.Core.DTOs.Shared; 
public class PayoutAccountJSONType {
    public Guid Id { get; set; } = Guid.NewGuid();
    public bool IsSaved { get; set; } = false;
    public required string PayoutMethodType { get; set; }
    public Dictionary<string, string> Fields { get; set; } = new();
}
