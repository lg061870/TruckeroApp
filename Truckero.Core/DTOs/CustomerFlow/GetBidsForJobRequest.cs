using System;
using System.Collections.Generic;

namespace Truckero.Core.DTOs.CustomerFlow
{
    public class GetBidsForJobRequest
    {
        public Guid JobId { get; set; }
        public string? Search { get; set; }
        public string? SortBy { get; set; }
        public string? SortDir { get; set; }
        public string? Help { get; set; }
        public string? Size { get; set; }
        public string? Category { get; set; }
        public List<string>? VehicleTypes { get; set; }
    }
}