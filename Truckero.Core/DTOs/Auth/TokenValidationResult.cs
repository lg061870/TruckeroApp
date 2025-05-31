using System.Text.Json.Serialization;

namespace Truckero.Core.DTOs.Auth;

public class TokenValidationResult
{
    [JsonPropertyName("valid")]
    public bool Valid { get; set; }

    [JsonPropertyName("reason")]
    public string? Reason { get; set; }
    public string? ErrorMessage { get; set; }
}
