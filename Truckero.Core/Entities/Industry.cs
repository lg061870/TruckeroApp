namespace Truckero.Core.Entities;

public class Industry
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    // Many-to-many join to companies via CompanyIndustry
    public ICollection<CompanyIndustry> Companies { get; set; } = new List<CompanyIndustry>();
}
