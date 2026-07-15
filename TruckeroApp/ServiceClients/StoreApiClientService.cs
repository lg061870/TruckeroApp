using System.Net;
using System.Net.Http.Json;
using Truckero.Core.DTOs;
using Truckero.Core.DTOs.Store;
using TruckeroApp.ServiceClients.ApiHelpers;

namespace TruckeroApp.ServiceClients;

public class StoreApiClientService : ApiClientBase, IStoreApiClientService
{
    public StoreApiClientService(HttpClient http) : base(http)
    {
    }

    public Task<StoreListResponse> GetStoresByCompanyIdAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        return GetRequiredAsync<StoreListResponse>(ApiRoutes.Store.CompanyStores(companyId), cancellationToken);
    }

    public Task<StoreResponse> CreateStoreAsync(StoreRequest request, CancellationToken cancellationToken = default)
    {
        return PostRequiredAsync<StoreResponse>(ApiRoutes.Store.Base, request, cancellationToken);
    }

    public Task<StoreResponse> UpdateStoreAsync(Guid id, StoreRequest request, CancellationToken cancellationToken = default)
    {
        return PutRequiredAsync<StoreResponse>(ApiRoutes.Store.ById(id), request, cancellationToken);
    }

    public async Task<BaseResponse> DeleteStoreAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await DeleteOptionalAsync<BaseResponse>(ApiRoutes.Store.ById(id), cancellationToken);
        return result ?? new BaseResponse { Success = true };
    }

    public async Task<BaseResponse> DisableStoreAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var response = await Http.PostAsync(ApiRoutes.Store.Disable(id), content: null, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            var payload = await response.Content.ReadFromJsonAsync<BaseResponse>(JsonOptions, cancellationToken);
            return payload ?? new BaseResponse { Success = true };
        }

        // Current API implementation does not expose this endpoint yet.
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return await DeleteStoreAsync(id, cancellationToken);
        }

        await ThrowApiExceptionAsync(response, $"POST {ApiRoutes.Store.Disable(id)}", cancellationToken);
        throw new InvalidOperationException("Unreachable after ThrowApiExceptionAsync.");
    }
}
