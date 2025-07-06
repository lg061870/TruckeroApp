using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Truckero.Core.Constants;
using Truckero.Core.DTOs.Common;
using Truckero.Core.DTOs.PayoutAccount;
using Truckero.Core.Entities;
using Truckero.Core.Interfaces.Services;

namespace TruckeroApp.ServiceClients;

public class PayoutAccountApiClientService : IPayoutAccountService {
    private readonly HttpClient _http;

    public PayoutAccountApiClientService(HttpClient http) {
        _http = http;
        Console.WriteLine($"[API Client] Hitting: {_http.BaseAddress}");
    }

    // --- Reference Data ---
    public async Task<PayoutAccountReferenceDataRequest> GetPayoutPageReferenceDataAsync(string? countryCode) {
        var code = string.IsNullOrWhiteSpace(countryCode) ? "CR" : countryCode.Trim().ToUpperInvariant();
        var response = await _http.GetAsync($"/api/viewprovider/payout-page-data?countryCode={code}");

        if (response.IsSuccessStatusCode) {
            var dto = await response.Content.ReadFromJsonAsync<PayoutAccountReferenceDataRequest>();
            return dto ?? new PayoutAccountReferenceDataRequest();
        }

        var content = await response.Content.ReadAsStringAsync();
        throw new Exception($"Failed to load payout page reference data: {content}");
    }

    // --- Chatty CRUD Operations ---

    public async Task<PayoutAccountResponse> GetPayoutAccountByIdAsync(Guid payoutAccountId) {
        var response = await _http.GetAsync($"/api/payoutaccount/{payoutAccountId}");
        return await ParseResponse(response);
    }

    public async Task<PayoutAccountResponse> GetPayoutAccountsByUserIdAsync(Guid userId) {
        var response = await _http.GetAsync($"/api/payoutaccount/user/{userId}");
        return await ParseResponse(response);
    }

    public async Task<PayoutAccountResponse> GetDefaultPayoutAccountByUserIdAsync(Guid userId) {
        var response = await _http.GetAsync($"/api/payoutaccount/user/{userId}/default");
        return await ParseResponse(response);
    }

    public async Task<PayoutAccountResponse> AddPayoutAccountAsync(Guid userId, PayoutAccountRequest payoutAccountRequest) {
        var response = await _http.PostAsJsonAsync($"/api/payoutaccount/user/{userId}", payoutAccountRequest);
        return await ParseResponse(response);
    }

    public async Task<PayoutAccountResponse> UpdatePayoutAccountAsync(Guid userId, Guid payoutAccountId, PayoutAccountRequest payoutAccountRequest) {
        var response = await _http.PutAsJsonAsync($"/api/payoutaccount/user/{userId}/{payoutAccountId}", payoutAccountRequest);
        return await ParseResponse(response);
    }

    public async Task<PayoutAccountResponse> DeletePayoutAccountAsync(Guid userId, Guid payoutAccountId) {
        var response = await _http.DeleteAsync($"/api/payoutaccount/user/{userId}/{payoutAccountId}");
        return await ParseResponse(response);
    }

    public async Task SetDefaultPayoutAccountAsync(Guid userId, Guid payoutAccountId) {
        var response = await _http.PostAsync($"/api/payoutaccount/user/{userId}/setdefault/{payoutAccountId}", null);

        if (!response.IsSuccessStatusCode) {
            var content = await response.Content.ReadAsStringAsync();
            // Optionally parse error payload for ErrorCode and Message
            throw new Exception($"Failed to set default payout account: {content}");
        }
    }

    // --- Payment Method Types (for reference) ---
    public async Task<List<PaymentMethodType>> GetAllPayoutPaymentMethodsAsync() {
        return await _http.GetFromJsonAsync<List<PaymentMethodType>>("/api/payoutaccount/types")
               ?? new List<PaymentMethodType>();
    }

    public async Task<List<PaymentMethodType>> GetAllPayoutPaymentMethodsByCountryCodeAsync(string countryCode) {
        var code = string.IsNullOrWhiteSpace(countryCode) ? "CR" : countryCode.Trim().ToUpperInvariant();
        return await _http.GetFromJsonAsync<List<PaymentMethodType>>($"/api/payoutaccount/types/{code}")
               ?? new List<PaymentMethodType>();
    }

    // --- Centralized API Response/Exception Handling ---
    private async Task<PayoutAccountResponse> ParseResponse(HttpResponseMessage response) {
        var content = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode) {
            var payload = await response.Content.ReadFromJsonAsync<PayoutAccountResponse>();
            if (payload != null)
                return payload;

            // Defensive: server returned 200 but null body
            return new PayoutAccountResponse {
                Success = false,
                Message = "API returned empty response.",
                ErrorCode = ExceptionCodes.Unknown
            };
        }

        // --- Try parse ValidationProblemDetails (422/400) ---
        if (response.StatusCode == HttpStatusCode.BadRequest &&
            response.Content.Headers.ContentType?.MediaType?.Contains("json") == true) {
            try {
                var validationDetails = JsonSerializer.Deserialize<ValidationProblemDetails>(content);
                if (validationDetails?.Errors?.Any() == true) {
                    var messages = validationDetails.Errors
                        .SelectMany(kvp => kvp.Value)
                        .Distinct()
                        .ToList();

                    return new PayoutAccountResponse {
                        Success = false,
                        Message = "Validation failed: " + string.Join("; ", messages),
                        ErrorCode = ExceptionCodes.ValidationFailed
                    };
                }
            } catch (JsonException) {
                // Fall through
            }
        }

        // --- Try parse structured API error ---
        try {
            var error = JsonSerializer.Deserialize<PayoutAccountResponse>(content);
            if (error != null && !string.IsNullOrWhiteSpace(error.ErrorCode)) {
                return error;
            }
        } catch (JsonException) {
            // Ignore, go to fallback
        }

        // --- Unrecognized error, build fallback response ---
        return new PayoutAccountResponse {
            Success = false,
            Message = $"API call failed: {response.StatusCode}. {content}",
            ErrorCode = ExceptionCodes.Unknown
        };
    }
}
