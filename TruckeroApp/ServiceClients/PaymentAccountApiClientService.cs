using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Truckero.Core.DTOs;
using Truckero.Core.DTOs.Common;
using Truckero.Core.DTOs.PaymentAccount;
using Truckero.Core.Entities;
using Truckero.Core.Exceptions;
using Truckero.Core.Interfaces.Services;
using TruckeroApp.Interfaces;

public class PaymentAccountApiClientService : IPaymentAccountService {
    private readonly HttpClient _http;
    private readonly IAuthSessionContext _session;
    private readonly ILogger<PaymentAccountApiClientService> _logger;

    public PaymentAccountApiClientService(HttpClient http, IAuthSessionContext session, ILogger<PaymentAccountApiClientService> logger) {
        _http = http;
        _session = session;
        _logger = logger;
    }

    private string RequireAccessToken()
        => _session.AccessToken ?? throw new UnauthorizedAccessException("No access token present in session.");

    // --------- IPaymentAccountService IMPLEMENTATION ---------
    public async Task<PaymentAccountResponse> GetPaymentAccountsByUserIdAsync(Guid userId) {
        var response = await _http.GetAsync($"/api/paymentaccount/user/{userId}");
        if (response.IsSuccessStatusCode)
            return await response.Content.ReadFromJsonAsync<PaymentAccountResponse>() ?? new PaymentAccountResponse();

        var content = await response.Content.ReadAsStringAsync();
        await HandleErrorResponse(response, content, "GetPaymentAccountsByUserIdAsync");
        return new PaymentAccountResponse();
    }

    public async Task<PaymentAccountResponse> GetPaymentAccountByIdAsync(Guid paymentAccountId) {
        var response = await _http.GetAsync($"/api/paymentaccount/{paymentAccountId}");
        if (response.IsSuccessStatusCode)
            return await response.Content.ReadFromJsonAsync<PaymentAccountResponse>() ?? new PaymentAccountResponse();

        var content = await response.Content.ReadAsStringAsync();
        await HandleErrorResponse(response, content, "GetPaymentAccountByIdAsync");
        return new PaymentAccountResponse();
    }

    public async Task<PaymentAccountResponse> AddPaymentAccountAsync(PaymentAccountRequest request) {
        var response = await _http.PostAsJsonAsync($"/api/paymentaccount", request);
        if (response.IsSuccessStatusCode)
            return await response.Content.ReadFromJsonAsync<PaymentAccountResponse>() ?? new PaymentAccountResponse();

        var content = await response.Content.ReadAsStringAsync();
        await HandleErrorResponse(response, content, "AddPaymentAccountAsync");
        return new PaymentAccountResponse();
    }

    public async Task<PaymentAccountResponse> UpdatePaymentAccountAsync(PaymentAccountRequest request) {
        // Assumes the ID is within the request object.
        var response = await _http.PutAsJsonAsync($"/api/paymentaccount/{request.Id}", request);
        if (response.IsSuccessStatusCode)
            return await response.Content.ReadFromJsonAsync<PaymentAccountResponse>() ?? new PaymentAccountResponse();

        var content = await response.Content.ReadAsStringAsync();
        await HandleErrorResponse(response, content, "UpdatePaymentAccountAsync");
        return new PaymentAccountResponse();
    }

    public async Task<PaymentAccountResponse> DeletePaymentAccountAsync(Guid userId, Guid paymentAccountId) {
        var response = await _http.DeleteAsync($"/api/paymentaccount/user/{userId}/{paymentAccountId}");
        if (response.IsSuccessStatusCode)
            return await response.Content.ReadFromJsonAsync<PaymentAccountResponse>() ?? new PaymentAccountResponse();

        var content = await response.Content.ReadAsStringAsync();
        await HandleErrorResponse(response, content, "DeletePaymentAccountAsync");
        return new PaymentAccountResponse();
    }

    public async Task<PaymentAccountResponse> SetDefaultPaymentAccountAsync(Guid userId, Guid paymentAccountId) {
        var response = await _http.PostAsync($"/api/paymentaccount/user/{userId}/setdefault/{paymentAccountId}", null);
        if (response.IsSuccessStatusCode)
            return await response.Content.ReadFromJsonAsync<PaymentAccountResponse>() ?? new PaymentAccountResponse();

        var content = await response.Content.ReadAsStringAsync();
        await HandleErrorResponse(response, content, "SetDefaultPaymentAccountAsync");
        return new PaymentAccountResponse();
    }

    public async Task<PaymentAccountResponse> MarkPaymentAccountValidatedAsync(Guid userId, Guid paymentAccountId) {
        var response = await _http.PostAsync($"/api/paymentaccount/user/{userId}/markvalidated/{paymentAccountId}", null);
        if (response.IsSuccessStatusCode)
            return await response.Content.ReadFromJsonAsync<PaymentAccountResponse>() ?? new PaymentAccountResponse();

        var content = await response.Content.ReadAsStringAsync();
        await HandleErrorResponse(response, content, "MarkPaymentAccountValidatedAsync");
        return new PaymentAccountResponse();
    }

    // --------- Optional: Reference Data, payout methods, etc. ---------
    public async Task<List<PaymentMethodType>> GetAllPayoutMethods(string countryCode) {
        var code = string.IsNullOrWhiteSpace(countryCode) ? "US" : countryCode.Trim().ToUpperInvariant();
        var response = await _http.GetAsync($"/api/paymentaccount/payout-types/{code}");
        if (response.IsSuccessStatusCode)
            return await response.Content.ReadFromJsonAsync<List<PaymentMethodType>>() ?? new List<PaymentMethodType>();

        var content = await response.Content.ReadAsStringAsync();
        await HandleErrorResponse(response, content, "GetAllPayoutMethods");
        return new List<PaymentMethodType>();
    }

    // --------- Error Handling ---------
    private async Task HandleErrorResponse(HttpResponseMessage response, string content, string operationName) {
        try {
            if (response.StatusCode == HttpStatusCode.BadRequest &&
                response.Content.Headers.ContentType?.MediaType?.Contains("json") == true) {
                var validationDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
                if (validationDetails?.Errors?.Any() == true) {
                    var messages = validationDetails.Errors
                        .SelectMany(kvp => kvp.Value)
                        .Distinct()
                        .ToList();

                    throw new PaymentAccountClientValidationException("Validation failed.", messages, response.StatusCode);
                }
            }

            var error = JsonSerializer.Deserialize<BaseResponse>(content);
            if (error != null && !string.IsNullOrWhiteSpace(error.Message)) {
                if (IsReferentialIntegrityCode(error.ErrorCode)) {
                    throw new ReferentialIntegrityClientException(error.Message, error.ErrorCode, response.StatusCode);
                }
                throw new PaymentAccountClientException(error.Message, error.ErrorCode, response.StatusCode);
            }
        } catch (JsonException) {
            // fall through to generic error below
        }

        throw new HttpRequestException($"API request for {operationName} failed. Status: {response.StatusCode}, Content: {content}", null, response.StatusCode);
    }

    private bool IsReferentialIntegrityCode(string? code) {
        if (string.IsNullOrWhiteSpace(code))
            return false;

        return code switch {
            "USER_NOT_FOUND" => true,
            "INVALID_PAYMENT_METHOD_TYPE" => true,
            "PAYMENT_ACCOUNT_NOT_FOUND" => true,
            "CANNOT_DELETE_DEFAULT" => true,
            _ => false
        };
    }
}
