using System;

namespace Truckero.Core.DTOs.CustomerFlow
{
    public class ConfirmDriverBidResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? ErrorCode { get; set; }
        public Guid JobId { get; set; }
        public Guid BidId { get; set; }
    }
}