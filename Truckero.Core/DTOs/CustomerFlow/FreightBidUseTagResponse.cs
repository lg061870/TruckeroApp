using System;
using System.Collections.Generic;

namespace Truckero.Core.DTOs.CustomerFlow; 
public class FreightBidUseTagResponse {
    public bool Success { get; set; }
    public string? Message { get; set; }
    public List<FreightBidUseTagRequest> FreightBidUseTags { get; set; } = new();
    public string? ErrorCode { get; set; }
}