using System;
using Truckero.Core.Entities;

namespace Truckero.Core.DTOs.Trucks;

using System.Text.Json.Serialization;

public class TruckResponse {
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("truck")]
    public Truck? Truck { get; set; }

    [JsonPropertyName("errorCode")]
    public string? ErrorCode { get; set; }
}
