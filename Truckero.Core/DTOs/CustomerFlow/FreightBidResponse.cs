namespace Truckero.Core.DTOs.CustomerFlow;

public class FreightBidResponse: BaseResponse {
    public List<FreightBidRequest> FreightBids { get; set; } = new();
}