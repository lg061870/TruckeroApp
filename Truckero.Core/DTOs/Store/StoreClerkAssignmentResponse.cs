using System.ComponentModel.DataAnnotations.Schema;

namespace Truckero.Core.DTOs;

public class StoreClerkAssignmentResponse {
    public bool Success { get; set; }
    public string? Message { get; set; }
    public List<StoreClerkAssignmentRequest> StoreClerkAssignmentRequests { get; set; } = new();
    public string? ErrorCode { get; set; }
}