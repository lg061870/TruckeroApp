using Truckero.Core.Entities;

namespace Truckero.Core.Interfaces.Repositories;

public interface IDriverBidRepository {
    Task<DriverBid> GetDriverBidByIdAsync(Guid id);
    Task<IReadOnlyList<DriverBid>> GetDriverBidsByFreightBidIdAsync(Guid freightBidId);
    Task<IReadOnlyList<DriverBid>> GetDriverBidsByDriverIdAsync(Guid driverId);
    Task<DriverBid> AddDriverBidAsync(DriverBid entity);
    Task<DriverBid> UpdateDriverBidAsync(DriverBid entity);
    Task DeleteDriverBidAsync(Guid id);
}
