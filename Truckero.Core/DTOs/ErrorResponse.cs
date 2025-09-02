using System.Text.Json.Serialization;

namespace Truckero.Core.DTOs;

public class BaseResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("errorcode")]
    public string? ErrorCode { get; set; }
}