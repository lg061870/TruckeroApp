namespace Truckero.Core.Entities;

// Join entity for many-to-many between Company and Industry
public class CompanyIndustry
{
    public Guid CompanyId { get; set; }
    public Company Company { get; set; } = null!;

    public Guid IndustryId { get; set; }
    public Industry Industry { get; set; } = null!;
}
