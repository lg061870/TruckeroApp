namespace Truckero.Core.DTOs;

public class StoreRequest
{
    public Guid Id { get; set; } // Added to support update vs create scenarios
    public Guid CompanyId { get; set; }
    public string Name { get; set; } = null!;
    public string? LegalEntity { get; set; }
    public string? DomainWhitelist { get; set; }
    public string? StoreType { get; set; }
    public Guid? ParentStoreId { get; set; }
    public string? Region { get; set; }
    public string? Address { get; set; }
}
