using System;
using System.Collections.Generic;

namespace Truckero.Core.DTOs.CustomerFlow
{
    public class GetBidsForJobResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? ErrorCode { get; set; }
        public List<BidDetailsDto> Bids { get; set; } = new List<BidDetailsDto>();
    }

    public class BidDetailsDto
    {
        public Guid BidId { get; set; }
        public Guid JobId { get; set; }
        public Guid DriverId { get; set; }
        public string DriverName { get; set; } = string.Empty;
        public string DriverAvatarUrl { get; set; } = string.Empty;
        public double Rating { get; set; }
        public int RatingCount { get; set; }
        public double DistanceMiles { get; set; }
        public string VehicleType { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Size { get; set; } = string.Empty;
        public string Help { get; set; } = string.Empty;
        public decimal PriceTotal { get; set; }
        public string Currency { get; set; } = "USD";
        public string Status { get; set; } = string.Empty;
        public DateTime LastUpdatedUtc { get; set; }
    }
}