using Truckero.Core.DTOs;
using TruckeroApp.ServiceClients.ApiHelpers;

namespace TruckeroApp.ServiceClients;

public class CompanyApiClientService : ApiClientBase, ICompanyApiClientService
{
    public CompanyApiClientService(HttpClient http) : base(http)
    {
    }

    public Task<CompanyResponse> CreateOrUpdateCompanyAsync(CompanyRequest request, CancellationToken cancellationToken = default)
    {
        return request.Id == Guid.Empty
            ? PostRequiredAsync<CompanyResponse>(ApiRoutes.Company.Base, request, cancellationToken)
            : PutRequiredAsync<CompanyResponse>(ApiRoutes.Company.ById(request.Id), request, cancellationToken);
    }

    public Task<CompanyResponse> LoadMetadataAsync(CancellationToken cancellationToken = default)
    {
        return GetRequiredAsync<CompanyResponse>(ApiRoutes.Company.Metadata, cancellationToken);
    }

    public Task<CompanyResponse> GetCompanyByIdAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        return GetRequiredAsync<CompanyResponse>(ApiRoutes.Company.ById(companyId), cancellationToken);
    }
}
