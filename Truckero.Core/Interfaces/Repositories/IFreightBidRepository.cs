// ---------------------- Truckero.Core.Repositories.IFreightBidRepository.cs ----------------------
namespace Truckero.Core.Interfaces.Repositories; 

public interface IFreightBidRepository {
    // FreightBid
    Task<FreightBid> GetFreightBidByIdAsync(Guid id);
    Task<IReadOnlyList<FreightBid>> GetAllFreightBidsAsync();
    Task<IReadOnlyList<FreightBid>> GetFreightBidsByCustomerIdAsync(Guid customerId);
    Task<FreightBid> AddFreightBidAsync(FreightBid entity);
    Task<FreightBid> UpdateFreightBidAsync(FreightBid entity);
    Task DeleteFreightBidAsync(Guid id);

    // FreightBidUseTag
    Task<FreightBidUseTag> GetFreightBidUseTagByIdAsync(Guid id);
    Task<IReadOnlyList<FreightBidUseTag>> GetFreightBidUseTagsByFreightBidIdAsync(Guid freightBidId);
    Task<IReadOnlyList<FreightBidUseTag>> GetFreightBidUseTagsByUseTagIdAsync(Guid useTagId);
    Task<FreightBidUseTag> AddFreightBidUseTagAsync(FreightBidUseTag entity);
    Task DeleteFreightBidUseTagAsync(Guid id);
}

