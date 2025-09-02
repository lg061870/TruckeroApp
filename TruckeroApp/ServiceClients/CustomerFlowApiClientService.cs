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

    private static readonly HashSet<string> ReferentialIntegrityCodes = new()
    {
        "CUSTOMER_NOT_FOUND",
        "TRUCK_TYPE_NOT_FOUND",
        "DRIVER_NOT_FOUND",
        "TRUCK_NOT_FOUND",
        "PAYMENT_METHOD_NOT_FOUND",
        "FREIGHTBID_NOT_FOUND",
        "FREIGHTBID_SAVE_FAILED",
        "FREIGHTBID_CONFLICT",
        "USE_TAG_NOT_FOUND",
        "FREIGHTBIDUSETAG_NOT_FOUND"
    };

    public CustomerFlowApiClientService(HttpClient http) {
        _http = http;
    }

    // -------------------- Freight Bid (Request Freight) --------------------

    public async Task<FreightBidResponse> CreateFreightBidAsync(FreightBidRequest request) {
        var response = await _http.PostAsJsonAsync("/api/FreightBid", request);

        if (response.IsSuccessStatusCode) {
            var result = await response.Content.ReadFromJsonAsync<FreightBidResponse>();
            if (result == null)
                throw new Exception("Empty FreightBidResponse");
            return result;
        }

        await HandleErrorResponse(response, "Failed to create freight bid.");
        throw new Exception("Unreachable");
    }

    public async Task<FreightBidDetailsResponse> GetFreightBidDetailsAsync(Guid freightBidId) {
        var response = await _http.GetAsync($"/api/FreightBid/{freightBidId}");

        if (response.IsSuccessStatusCode) {
            var result = await response.Content.ReadFromJsonAsync<FreightBidDetailsResponse>();
            if (result == null)
                throw new Exception("Empty FreightBidDetailsResponse");
            return result;
        }

        await HandleErrorResponse(response, "Failed to get freight bid details.");
        throw new Exception("Unreachable");
    }

    public async Task<List<BidHistoryResponse>> GetBidHistoryAsync(Guid customerId) {
        var response = await _http.GetAsync($"/api/FreightBid/customer/{customerId}");

        if (response.IsSuccessStatusCode) {
            var result = await response.Content.ReadFromJsonAsync<List<BidHistoryResponse>>();
            return result ?? new List<BidHistoryResponse>();
        }

        await HandleErrorResponse(response, "Failed to get bid history.");
        throw new Exception("Unreachable");
    }

    public async Task<FindDriversStatusResponse> GetFindDriversStatusAsync(Guid freightBidId) {
        var response = await _http.GetAsync($"/api/FreightBid/{freightBidId}/find-drivers-status");

        if (response.IsSuccessStatusCode) {
            var result = await response.Content.ReadFromJsonAsync<FindDriversStatusResponse>();
            if (result == null)
                throw new Exception("Empty FindDriversStatusResponse");
            return result;
        }

        await HandleErrorResponse(response, "Failed to get find drivers status.");
        throw new Exception("Unreachable");
    }

    // -------------------- Driver Bids (Choose/Manage Driver) --------------------

    public async Task<List<DriverBidResponse>> GetDriverBidsForFreightBidAsync(Guid freightBidId) {
        var response = await _http.GetAsync($"/api/FreightBid/{freightBidId}/driver-bids");

        if (response.IsSuccessStatusCode) {
            var result = await response.Content.ReadFromJsonAsync<List<DriverBidResponse>>();
            return result ?? new List<DriverBidResponse>();
        }

        await HandleErrorResponse(response, "Failed to get driver bids.");
        throw new Exception("Unreachable");
    }

    public async Task AssignDriverAsync(AssignDriverRequest request) {
        var response = await _http.PostAsJsonAsync("/api/FreightBid/assign-driver", request);

        if (response.IsSuccessStatusCode)
            return;

        await HandleErrorResponse(response, "Failed to assign driver.");
    }

    public async Task<DriverBidResponse> GetDriverBidDetailsAsync(Guid bidId) {
        var response = await _http.GetAsync($"/api/FreightBid/driver-bid/{bidId}");

        if (response.IsSuccessStatusCode) {
            var result = await response.Content.ReadFromJsonAsync<DriverBidResponse>();
            if (result == null)
                throw new Exception("Empty DriverBidResponse");
            return result;
        }

        await HandleErrorResponse(response, "Failed to get driver bid details.");
        throw new Exception("Unreachable");
    }

    // -------------------- Exception Handling Helper --------------------

    private static async Task HandleErrorResponse(HttpResponseMessage response, string defaultMessage) {
        var content = await response.Content.ReadAsStringAsync();

        try {
            // Validation errors
            if (response.StatusCode == HttpStatusCode.BadRequest ||
                response.StatusCode == HttpStatusCode.UnprocessableEntity) {
                if (response.Content.Headers.ContentType?.MediaType?.Contains("json") == true) {
                    var validationDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
                    if (validationDetails?.Errors?.Any() == true) {
                        var messages = validationDetails.Errors
                            .SelectMany(kvp => kvp.Value)
                            .Distinct()
                            .ToList();

                        throw new CustomerFlowValidationException("Validation failed.", messages, response.StatusCode);
                    }
                }
            }

            // Structured backend error
            var error = JsonSerializer.Deserialize<FreightBidResponse>(content);

            if (error != null && !string.IsNullOrWhiteSpace(error.ErrorCode)) {
                if (ReferentialIntegrityCodes.Contains(error.ErrorCode)) {
                    throw new ReferentialIntegrityClientException(
                        error.ErrorCode ?? defaultMessage, error.ErrorCode, response.StatusCode);
                }
                throw new CustomerFlowClientException(
                    error.ErrorCode ?? defaultMessage, error.ErrorCode, response.StatusCode);
            }

            // Fallback to generic error if no details parsed
            throw new HttpRequestException($"{defaultMessage}. Status: {response.StatusCode} - {content}", null, response.StatusCode);
        } catch (JsonException) {
            // Fallback: If parsing error details failed, throw as generic HTTP error
            throw new HttpRequestException($"{defaultMessage}. Status: {response.StatusCode} - {content}", null, response.StatusCode);
        }
    }
}
