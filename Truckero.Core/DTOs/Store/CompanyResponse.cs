namespace Truckero.Core.DTOs;

public class CompanyResponse {
    public bool Success { get; set; }
    public string? Message { get; set; }
    public List<CompanyRequest> CompanyRequests { get; set; } = new();
    public List<BusinessTypeRequest> BusinessTypes { get; set; } = new();
    public List<IndustryTypeRequest> IndustryTypes { get; set; } = new();
    public string? ErrorCode { get; set; }
}
