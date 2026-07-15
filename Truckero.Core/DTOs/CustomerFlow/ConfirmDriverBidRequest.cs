using System;
using System.Collections.Generic;

namespace Truckero.Core.DTOs.CustomerFlow
{
    public class ConfirmDriverBidRequest
    {
        public Guid JobId { get; set; }
        public Guid BidId { get; set; }
    }
}