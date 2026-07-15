using System;
using System.Collections.Generic;
using Truckero.Core.DTOs.CustomerFlow;

namespace TruckeroApp.Models.CustomerFlow
{
    // Request model for fetching bids for a job
    public class GetBidsForJobRequest
    {
        public Guid JobId { get; set; }
        public string? Search { get; set; }
        public string? SortBy { get; set; } = "price";
        public string? SortDir { get; set; } = "asc";
        public string? Help { get; set; }
        public string? Size { get; set; }
        public string? Category { get; set; }
        public List<string>? VehicleTypes { get; set; }
    }

    // Response model for fetching bids for a job
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

    // Request model for confirming a driver bid
    public class ConfirmDriverBidRequest
    {
        public Guid JobId { get; set; }
        public Guid BidId { get; set; }
    }

    // Response model for confirming a driver bid
    public class ConfirmDriverBidResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? ErrorCode { get; set; }
        public Guid JobId { get; set; }
        public Guid BidId { get; set; }
    }

    // Request model for requesting more drivers
    public class RequestMoreDriversRequest
    {
        public Guid JobId { get; set; }
        public double DistanceMilesMax { get; set; } = 25;
        public List<string>? VehicleTypes { get; set; }
        public string? Help { get; set; }
        public string? Size { get; set; }
        public string? Category { get; set; }
    }

    // Response model for requesting more drivers
    public class RequestMoreDriversResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? ErrorCode { get; set; }
    }

    // Filter state for UI
    public class FilterState
    {
        public string? Search { get; set; }
        public string SortBy { get; set; } = "price";
        public string SortDir { get; set; } = "asc";
        public string? Help { get; set; }
        public string? Size { get; set; }
        public string? Category { get; set; }
        public List<string>? VehicleTypes { get; set; }
        public string DeliveryType { get; set; } = "Express";
    }
}