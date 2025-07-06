using Truckero.Core.DTOs.CustomerFlow;

namespace Truckero.Core.Services;

public interface IFreightBidService {
    // FreightBid
    Task<FreightBidResponse> GetFreightBidByIdAsync(Guid id);
    Task<FreightBidResponse> GetAllFreightBidsAsync();
    Task<FreightBidResponse> GetFreightBidsByCustomerIdAsync(Guid customerId);
    Task<FreightBidResponse> CreateFreightBidAsync(FreightBidRequest request);
    Task<FreightBidResponse> UpdateFreightBidAsync(Guid id, FreightBidRequest request);
    Task<FreightBidResponse> DeleteFreightBidAsync(Guid id);

    // FreightBidUseTag
    Task<FreightBidUseTagResponse> GetFreightBidUseTagByIdAsync(Guid id);
    Task<FreightBidUseTagResponse> GetFreightBidUseTagsByFreightBidIdAsync(Guid freightBidId);
    Task<FreightBidUseTagResponse> GetFreightBidUseTagsByUseTagIdAsync(Guid useTagId);
    Task<FreightBidUseTagResponse> CreateFreightBidUseTagAsync(FreightBidUseTagRequest request);
    Task<FreightBidUseTagResponse> DeleteFreightBidUseTagAsync(Guid id);
}
