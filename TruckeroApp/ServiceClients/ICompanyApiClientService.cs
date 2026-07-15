using Truckero.Core.DTOs;

namespace TruckeroApp.ServiceClients;

public interface ICompanyApiClientService
{
    Task<CompanyResponse> CreateOrUpdateCompanyAsync(CompanyRequest request, CancellationToken cancellationToken = default);
    Task<CompanyResponse> LoadMetadataAsync(CancellationToken cancellationToken = default);
    Task<CompanyResponse> GetCompanyByIdAsync(Guid companyId, CancellationToken cancellationToken = default);
}
