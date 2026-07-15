using Truckero.Core.DTOs;
using Truckero.Core.DTOs.Store;

namespace Truckero.Core.Interfaces.Services;

public interface IStoreService
{
    Task<StoreResponse> CreateStoreAsync(StoreRequest request);
    Task<StoreResponse> UpdateStoreAsync(Guid id, StoreRequest request);
    Task<BaseResponse> DeleteStoreAsync(Guid id);
    Task<StoreResponse> GetStoreByIdAsync(Guid id);
    Task<StoreListResponse> GetStoresByCompanyIdAsync(Guid companyId);
}
