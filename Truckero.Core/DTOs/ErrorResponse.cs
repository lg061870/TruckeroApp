using System.Text.Json.Serialization;

namespace Truckero.Core.DTOs;

public class ErrorResponse
{
    [JsonPropertyName("error")]
    public string? Error { get; set; }

    [JsonPropertyName("code")]
    public string? Code { get; set; }
}