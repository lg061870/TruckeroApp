namespace Truckero.Core.DTOs.CustomerFlow;

public class AssignDriverResponse {
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? ErrorCode { get; set; }
    public List<AssignDriverRequest> Assignments { get; set; } = new();
}
