using System;
using Truckero.Core.Entities;

namespace Truckero.Core.DTOs.Trucks;

public class TruckResponseDto
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public Truck? Truck { get; set; }
    public string ErrorCode { get; set; }
}