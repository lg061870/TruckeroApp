using System;
using System.Collections.Generic;
using System.Linq;

namespace Truckero.Diagnostics.Mocks.Services
{
    /// <summary>
    /// Model for storing bid details used in the FreightBidQueue component
    /// </summary>
    public class BidDetailsDto
    {
        public Guid BidId { get; set; }
        public Guid DriverId { get; set; }
        public string DriverName { get; set; } = string.Empty;
        public string DriverAvatarUrl { get; set; } = string.Empty;
        public decimal PriceTotal { get; set; }
        public decimal PriceBase { get; set; }
        public decimal ServiceFee { get; set; }
        public decimal Tax { get; set; }
        public int RatingCount { get; set; }
        public double Rating { get; set; }
        public double DistanceMiles { get; set; }
        public int EstimatedArrivalMinutes { get; set; }
        public string VehicleType { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Size { get; set; } = string.Empty;
        public string Help { get; set; } = string.Empty;
        public string Comment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}