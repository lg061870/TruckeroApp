// File: Truckero.Core.Interfaces.Services/IDriverBidService.cs
using Truckero.Core.DTOs.CustomerFlow;

namespace Truckero.Core.Interfaces.Services;

public interface IDriverBidService {
    Task<DriverBidResponse> CreateDriverBidAsync(DriverBidRequest request);
    Task<DriverBidResponse> UpdateDriverBidAsync(Guid id, DriverBidRequest request);
    Task<DriverBidResponse> GetDriverBidByIdAsync(Guid id);
    Task<DriverBidResponse> GetDriverBidsByFreightBidIdAsync(Guid freightBidId);
    Task<DriverBidResponse> GetDriverBidsByDriverIdAsync(Guid driverId);
    Task<DriverBidResponse> DeleteDriverBidAsync(Guid id);
    Task<FindDriversStatusResponse> GetFindDriversStatusAsync(Guid freightBidId);
}
