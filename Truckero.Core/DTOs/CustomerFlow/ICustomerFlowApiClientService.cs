using Truckero.Core.DTOs.CustomerFlow;

namespace TruckeroApp.ServiceClients;

// You may want to put this interface elsewhere, but for completeness:
public interface ICustomerFlowApiClientService {
    Task<FreightBidResponse> CreateFreightBidAsync(FreightBidRequest request);
    Task<FreightBidDetailsResponse> GetFreightBidDetailsAsync(Guid freightBidId);
    Task<List<BidHistoryItemResponse>> GetBidHistoryAsync(Guid customerId);
    Task<FindDriversStatusResponse> GetFindDriversStatusAsync(Guid freightBidId);

    Task<List<DriverBidResponse>> GetDriverBidsForFreightBidAsync(Guid freightBidId);
    Task AssignDriverAsync(AssignDriverRequest request);
    Task<DriverBidResponse> GetDriverBidDetailsAsync(Guid bidId);

    // Add more as your flow grows, e.g. CancelBid, TrackDriver, etc.
}
