using System;
using System.Collections.Generic;

namespace Truckero.Core.DTOs.CustomerFlow
{
    public class RequestMoreDriversRequest
    {
        public Guid JobId { get; set; }
        public double DistanceMilesMax { get; set; }
        public List<string>? VehicleTypes { get; set; }
        public string? Help { get; set; }
        public string? Size { get; set; }
        public string? Category { get; set; }
    }
}