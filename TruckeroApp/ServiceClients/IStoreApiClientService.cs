using Truckero.Core.DTOs;
using Truckero.Core.DTOs.Store;

namespace TruckeroApp.ServiceClients;

public interface IStoreApiClientService
{
    Task<StoreListResponse> GetStoresByCompanyIdAsync(Guid companyId, CancellationToken cancellationToken = default);
    Task<StoreResponse> CreateStoreAsync(StoreRequest request, CancellationToken cancellationToken = default);
    Task<StoreResponse> UpdateStoreAsync(Guid id, StoreRequest request, CancellationToken cancellationToken = default);
    Task<BaseResponse> DeleteStoreAsync(Guid id, CancellationToken cancellationToken = default);
    Task<BaseResponse> DisableStoreAsync(Guid id, CancellationToken cancellationToken = default);
}
