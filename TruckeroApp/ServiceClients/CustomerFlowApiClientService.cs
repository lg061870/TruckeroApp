using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Truckero.Core.DTOs.CustomerFlow;
using Truckero.Core.DTOs.Common;
using Truckero.Core.Exceptions;
using Truckero.Core.DTOs;

namespace TruckeroApp.ServiceClients;

public class CustomerFlowApiClientService : ICustomerFlowApiClientService {
    private readonly HttpClient _http;
    // private readonly IAuthSessionContext _session;

    public CustomerFlowApiClientService(HttpClient http/*, IAuthSessionContext session*/) {
        _http = http;
        // _session = session;
    }

    // -------------------- Freight Bid (Request Freight) --------------------

    public async Task<FreightBidResponse> CreateFreightBidAsync(FreightBidRequest request) {
        // var envelope = AuthenticatedEnvelope.Create(RequireAccessToken(), _http, HttpMethod.Post, "/customerflow/freight-bid", request);
        // var response = await envelope.SendAsync<HttpResponseMessage>();
        var response = await _http.PostAsJsonAsync("/customerflow/freight-bid", request);

        if (response.IsSuccessStatusCode) {
            return await response.Content.ReadFromJsonAsync<FreightBidResponse>()
                ?? throw new Exception("Empty FreightBidResponse");
        }

        await ThrowCustomerFlowException(response, "Failed to create freight bid.");
        throw new Exception("Unreachable"); // never hit
    }

    public async Task<FreightBidDetailsResponse> GetFreightBidDetailsAsync(Guid freightBidId) {
        var response = await _http.GetAsync($"/customerflow/freight-bid/{freightBidId}");

        if (response.IsSuccessStatusCode) {
            return await response.Content.ReadFromJsonAsync<FreightBidDetailsResponse>()
                ?? throw new Exception("Empty FreightBidDetailsResponse");
        }

        await ThrowCustomerFlowException(response, "Failed to get freight bid details.");
        throw new Exception("Unreachable");
    }

    public async Task<List<BidHistoryItemResponse>> GetBidHistoryAsync(Guid customerId) {
        var response = await _http.GetAsync($"/customerflow/bid-history/{customerId}");

        if (response.IsSuccessStatusCode) {
            return await response.Content.ReadFromJsonAsync<List<BidHistoryItemResponse>>()
                ?? new List<BidHistoryItemResponse>();
        }

        await ThrowCustomerFlowException(response, "Failed to get bid history.");
        throw new Exception("Unreachable");
    }

    public async Task<FindDriversStatusResponse> GetFindDriversStatusAsync(Guid freightBidId) {
        var response = await _http.GetAsync($"/customerflow/find-drivers-status/{freightBidId}");

        if (response.IsSuccessStatusCode) {
            return await response.Content.ReadFromJsonAsync<FindDriversStatusResponse>()
                ?? throw new Exception("Empty FindDriversStatusResponse");
        }

        await ThrowCustomerFlowException(response, "Failed to get find drivers status.");
        throw new Exception("Unreachable");
    }

    // -------------------- Driver Bids (Choose/Manage Driver) --------------------

    public async Task<List<DriverBidResponse>> GetDriverBidsForFreightBidAsync(Guid freightBidId) {
        var response = await _http.GetAsync($"/customerflow/freight-bid/{freightBidId}/driver-bids");

        if (response.IsSuccessStatusCode) {
            return await response.Content.ReadFromJsonAsync<List<DriverBidResponse>>()
                ?? new List<DriverBidResponse>();
        }

        await ThrowCustomerFlowException(response, "Failed to get driver bids.");
        throw new Exception("Unreachable");
    }

    public async Task AssignDriverAsync(AssignDriverRequest request) {
        var response = await _http.PostAsJsonAsync("/customerflow/assign-driver", request);

        if (response.IsSuccessStatusCode)
            return;

        await ThrowCustomerFlowException(response, "Failed to assign driver.");
    }

    public async Task<DriverBidResponse> GetDriverBidDetailsAsync(Guid bidId) {
        var response = await _http.GetAsync($"/customerflow/driver-bid/{bidId}");

        if (response.IsSuccessStatusCode) {
            return await response.Content.ReadFromJsonAsync<DriverBidResponse>()
                ?? throw new Exception("Empty DriverBidResponse");
        }

        await ThrowCustomerFlowException(response, "Failed to get driver bid details.");
        throw new Exception("Unreachable");
    }

    // -------------------- Exception Handling Helper --------------------

    private static async Task ThrowCustomerFlowException(HttpResponseMessage response, string defaultMessage) {
        var content = await response.Content.ReadAsStringAsync();

        // Validation errors
        if (response.StatusCode == HttpStatusCode.BadRequest ||
            response.StatusCode == HttpStatusCode.UnprocessableEntity) {
            if (response.Content.Headers.ContentType?.MediaType?.Contains("json") == true) {
                try {
                    var validationDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
                    if (validationDetails?.Errors?.Any() == true) {
                        var messages = validationDetails.Errors
                            .SelectMany(kvp => kvp.Value)
                            .Distinct()
                            .ToList();

                        throw new CustomerFlowValidationException("Validation failed.", messages, response.StatusCode);
                    }
                } catch (JsonException) { }
            }
        }

        // Structured error
        try {
            var error = JsonSerializer.Deserialize<ErrorResponse>(content);
            if (error != null && !string.IsNullOrWhiteSpace(error.Code)) {
                if (IsReferentialIntegrityCode(error.Code)) {
                    throw new ReferentialIntegrityClientException(
                        error.Error ?? defaultMessage, error.Code, response.StatusCode);
                }
                throw new CustomerFlowClientException(
                    error.Error ?? defaultMessage, error.Code, response.StatusCode);
            }
        } catch (JsonException) { }

        throw new HttpRequestException($"{defaultMessage}. Status: {response.StatusCode} - {content}", null, response.StatusCode);
    }

    private static bool IsReferentialIntegrityCode(string? code) {
        return code switch {
            "FOREIGN_KEY_NOT_FOUND" => true,
            "REFERENTIAL_INTEGRITY_VIOLATION" => true,
            "TRUCK_TYPE_NOT_FOUND" => true,
            "PAYMENT_METHOD_NOT_FOUND" => true,
            // add other codes as you define them
            _ => false
        };
    }
}
