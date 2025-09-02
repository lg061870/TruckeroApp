public interface IFreightBidRepository {
    // FreightBid core
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

    // Bid History
    Task<IReadOnlyList<FreightBid>> GetBidHistoryByCustomerIdAsync(Guid customerId);

    // Assigning a driver (this updates the FreightBid)
    Task<bool> AssignDriverAsync(Guid freightBidId, Guid driverBidId);
}
