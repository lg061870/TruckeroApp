using System;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using TruckeroApp.Models.CustomerFlow;
using Truckero.Core.Exceptions;

namespace TruckeroApp.Services.CustomerFlow
{
    // Interface for API client
    public interface IFreightBidQueueService
    {
        Task<GetBidsForJobResponse> GetBidsForJobAsync(GetBidsForJobRequest request);
        Task<ConfirmDriverBidResponse> ConfirmDriverBidAsync(ConfirmDriverBidRequest request);
        Task<RequestMoreDriversResponse> RequestMoreDriversAsync(RequestMoreDriversRequest request);
    }

    // Implementation of API client
    public class FreightBidQueueService : IFreightBidQueueService
    {
        private readonly HttpClient _http;
        
        private static readonly HashSet<string> ReferentialIntegrityCodes = new()
        {
            "FREIGHTBID_NOT_FOUND",
            "DRIVER_NOT_FOUND",
            "ALREADY_CONFIRMED",
            "BID_NOT_FOUND"
        };

        public FreightBidQueueService(HttpClient http)
        {
            _http = http;
        }

        public async Task<GetBidsForJobResponse> GetBidsForJobAsync(GetBidsForJobRequest request)
        {
            var response = await _http.PostAsJsonAsync("/api/freightbid/job/responses", request);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<GetBidsForJobResponse>();
                return result ?? new GetBidsForJobResponse { Success = true, Message = "No bids found", Bids = new List<BidDetailsDto>() };
            }

            await HandleErrorResponse(response, "Failed to get bids for job.");
            throw new Exception("Unreachable code - HandleErrorResponse should throw");
        }

        public async Task<ConfirmDriverBidResponse> ConfirmDriverBidAsync(ConfirmDriverBidRequest request)
        {
            var response = await _http.PostAsJsonAsync("/api/freightbid/confirm", request);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ConfirmDriverBidResponse>();
                return result ?? new ConfirmDriverBidResponse { Success = true, Message = "Bid confirmed" };
            }

            await HandleErrorResponse(response, "Failed to confirm driver bid.");
            throw new Exception("Unreachable code - HandleErrorResponse should throw");
        }

        public async Task<RequestMoreDriversResponse> RequestMoreDriversAsync(RequestMoreDriversRequest request)
        {
            var response = await _http.PostAsJsonAsync("/api/driverbid/request-more", request);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<RequestMoreDriversResponse>();
                return result ?? new RequestMoreDriversResponse { Success = true, Message = "Additional drivers requested." };
            }

            await HandleErrorResponse(response, "Failed to request more drivers.");
            throw new Exception("Unreachable code - HandleErrorResponse should throw");
        }

        private async Task HandleErrorResponse(HttpResponseMessage response, string defaultMessage)
        {
            var content = await response.Content.ReadAsStringAsync();

            try
            {
                // Special handling for conflict on bid confirmation
                if (response.StatusCode == HttpStatusCode.Conflict)
                {
                    var conflictError = await response.Content.ReadFromJsonAsync<ConfirmDriverBidResponse>();
                    if (conflictError?.ErrorCode == "AlreadyConfirmed")
                    {
                        throw new CustomerFlowClientException(
                            "Driver already selected for this job.", "AlreadyConfirmed", response.StatusCode);
                    }
                }

                // Structured backend error
                if (response.Content.Headers.ContentType?.MediaType?.Contains("json") == true)
                {
                    var errorResponse = JsonSerializer.Deserialize<JsonElement>(content);
                    if (errorResponse.TryGetProperty("errorCode", out var errorCodeElement))
                    {
                        string? errorCode = errorCodeElement.GetString();
                        string message = errorResponse.TryGetProperty("message", out var messageElement) 
                            ? messageElement.GetString() ?? defaultMessage 
                            : defaultMessage;

                        if (!string.IsNullOrWhiteSpace(errorCode))
                        {
                            if (ReferentialIntegrityCodes.Contains(errorCode))
                            {
                                throw new ReferentialIntegrityClientException(message, errorCode, response.StatusCode);
                            }
                            throw new CustomerFlowClientException(message, errorCode, response.StatusCode);
                        }
                    }
                }

                // Fallback to generic error
                throw new HttpRequestException($"{defaultMessage}. Status: {response.StatusCode}", null, response.StatusCode);
            }
            catch (JsonException)
            {
                throw new HttpRequestException($"{defaultMessage}. Status: {response.StatusCode} - {content}", null, response.StatusCode);
            }
        }
    }
}