using Truckero.Core.DTOs.CustomerFlow;
using Truckero.Core.DTOs.Common;

namespace Truckero.Core.Services;

public interface IFreightBidService {
    // --- Core FreightBid ---
    Task<FreightBidResponse> CreateFreightBidAsync(FreightBidRequest request);
    Task<FreightBidResponse> UpdateFreightBidAsync(Guid freightBidId, FreightBidRequest request);
    Task<FreightBidResponse> DeleteFreightBidAsync(Guid freightBidId);
    Task<FreightBidDetailsResponse> GetFreightBidDetailsAsync(Guid freightBidId);
    Task<FreightBidResponse> GetAllFreightBidsAsync();

    // --- UseTag Features ---
    Task<FreightBidUseTagResponse> CreateFreightBidUseTagAsync(FreightBidUseTagRequest request);
    Task<FreightBidUseTagResponse> DeleteFreightBidUseTagAsync(Guid id);
    Task<FreightBidUseTagResponse> GetFreightBidUseTagByIdAsync(Guid id);
    Task<FreightBidUseTagResponse> GetFreightBidUseTagsByFreightBidIdAsync(Guid freightBidId);
    Task<FreightBidUseTagResponse> GetFreightBidUseTagsByUseTagIdAsync(Guid useTagId);

    // --- Customer Bid History ---
    Task<BidHistoryResponse> GetBidHistoryAsync(Guid customerId);

    // --- Find Drivers Status ---
    Task<FindDriversStatusResponse> GetFindDriversStatusAsync(Guid freightBidId);

    // --- Assign Driver ---
    Task<AssignDriverResponse> AssignDriverAsync(AssignDriverRequest request);
}
