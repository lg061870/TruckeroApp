using System.Net.Http.Json;
using System.Text.Json;
using Truckero.Core.DTOs;
using Truckero.Core.DTOs.Auth;
using Truckero.Core.DTOs.CustomerFlow;
using Truckero.Core.DTOs.PaymentMethodType;

namespace Truckero.Store.ServiceClients;

public sealed class StoreApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _http;

    public StoreApiClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<CompanyResponse> GetCompanyMetadataAsync(CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync<CompanyResponse>("/api/Company/metadata", cancellationToken)
            ?? new CompanyResponse { Success = false, Message = "Metadata payload was empty." };
    }

    public async Task<CompanyResponse> SaveCompanyAsync(CompanyRequest request, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response;

        if (request.Id == Guid.Empty)
        {
            response = await _http.PostAsJsonAsync("/api/Company", request, cancellationToken);
        }
        else
        {
            response = await _http.PutAsJsonAsync($"/api/Company/{request.Id}", request, cancellationToken);
        }

        return await ReadResponseOrThrowAsync<CompanyResponse>(response, cancellationToken)
            ?? new CompanyResponse { Success = false, Message = "Company response was empty." };
    }

    public async Task<CompanyResponse?> GetCompanyByIdAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync<CompanyResponse>($"/api/Company/{companyId}", cancellationToken);
    }

    public async Task<StoreListResponse> GetStoresByCompanyIdAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync<StoreListResponse>($"/api/Store/company/{companyId}", cancellationToken)
            ?? new StoreListResponse { Success = false, Message = "Store list response was empty." };
    }

    public async Task<StoreResponse> SaveStoreAsync(StoreRequest request, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response;

        if (request.Id == Guid.Empty)
        {
            response = await _http.PostAsJsonAsync("/api/Store", request, cancellationToken);
        }
        else
        {
            response = await _http.PutAsJsonAsync($"/api/Store/{request.Id}", request, cancellationToken);
        }

        return await ReadResponseOrThrowAsync<StoreResponse>(response, cancellationToken)
            ?? new StoreResponse { Success = false, Message = "Store response was empty." };
    }

    public async Task<FreightBidReferenceData> GetFreightBidReferenceDataAsync(CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync<FreightBidReferenceData>("/api/ViewProvider/freight-bid-reference-data", cancellationToken)
            ?? new FreightBidReferenceData();
    }

    public async Task<PaymentMethodTypeResponse> GetPaymentMethodTypesAsync(CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync<PaymentMethodTypeResponse>("/api/PaymentMethodType", cancellationToken)
            ?? new PaymentMethodTypeResponse { Success = false, Message = "Payment method response was empty." };
    }

    public async Task<IReadOnlyList<FreightBidRequest>> GetFreightBidsAsync(CancellationToken cancellationToken = default)
    {
        var response = await GetJsonAsync<FreightBidResponse>("/api/FreightBid", cancellationToken);
        return response?.FreightBids ?? new List<FreightBidRequest>();
    }

    public async Task<FreightBidDetailsRequest?> GetFreightBidDetailsAsync(Guid freightBidId, CancellationToken cancellationToken = default)
    {
        var response = await GetJsonAsync<FreightBidDetailsResponse>($"/api/FreightBid/{freightBidId}", cancellationToken);
        return response?.FreightBidDetails?.FirstOrDefault();
    }

    public async Task<DriverBidResponse?> GetDriverBidsForFreightBidAsync(Guid freightBidId, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync<DriverBidResponse>($"/api/FreightBid/{freightBidId}/driver-bids", cancellationToken);
    }

    public async Task<FreightBidResponse> CreateFreightBidAsync(FreightBidRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _http.PostAsJsonAsync("/api/FreightBid", request, cancellationToken);
        return await ReadResponseOrThrowAsync<FreightBidResponse>(response, cancellationToken)
            ?? new FreightBidResponse { Success = false, Message = "Freight bid create response was empty." };
    }

    public async Task<BaseResponse> AssignDriverAsync(AssignDriverRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _http.PostAsJsonAsync("/api/FreightBid/assign-driver", request, cancellationToken);
        return await ReadResponseOrThrowAsync<BaseResponse>(response, cancellationToken)
            ?? new BaseResponse { Success = response.IsSuccessStatusCode };
    }

    public async Task<BaseResponse> RegisterStoreClerkAsync(RegisterUserRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _http.PostAsJsonAsync("/auth/register", request, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return new BaseResponse { Success = true, Message = "Store clerk account created." };
        }

        await ThrowWithBodyAsync(response, cancellationToken);
        return new BaseResponse { Success = false, Message = "Unexpected register response." };
    }

    private async Task<T?> GetJsonAsync<T>(string path, CancellationToken cancellationToken)
    {
        using var response = await _http.GetAsync(path, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            await ThrowWithBodyAsync(response, cancellationToken);
        }

        return await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken);
    }

    private async Task<T?> ReadResponseOrThrowAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        using var disposable = response;
        if (!response.IsSuccessStatusCode)
        {
            await ThrowWithBodyAsync(response, cancellationToken);
        }

        return await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken);
    }

    private static async Task ThrowWithBodyAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        try
        {
            var baseResponse = JsonSerializer.Deserialize<BaseResponse>(body, JsonOptions);
            if (baseResponse is not null && (!string.IsNullOrWhiteSpace(baseResponse.ErrorCode) || !string.IsNullOrWhiteSpace(baseResponse.Message)))
            {
                throw new HttpRequestException(
                    $"API call failed ({(int)response.StatusCode}): {baseResponse.ErrorCode ?? "NO_CODE"} {baseResponse.Message}" ,
                    null,
                    response.StatusCode);
            }
        }
        catch (JsonException)
        {
            // Fall through to raw response body.
        }

        throw new HttpRequestException(
            $"API call failed ({(int)response.StatusCode}): {body}",
            null,
            response.StatusCode);
    }
}

