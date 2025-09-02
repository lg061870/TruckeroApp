namespace Truckero.Core.DTOs;

public class UserResponse {
    public bool Success { get; set; }
    public string? Message { get; set; }
    public List<UserRequest> Users { get; set; } = new();
    public string? ErrorCode { get; set; }
}