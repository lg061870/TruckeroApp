namespace Truckero.Core.Entities;

public class Store
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? LegalEntity { get; set; }
    public string? DomainWhitelist { get; set; }
    public ICollection<StoreClerkProfile> Clerks { get; set; } = new List<StoreClerkProfile>();
}




