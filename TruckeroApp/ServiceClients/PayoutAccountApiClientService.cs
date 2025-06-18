using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Truckero.Core.DTOs;
using Truckero.Core.DTOs.Common;
using Truckero.Core.DTOs.Onboarding;
using Truckero.Core.Entities;
using Truckero.Core.Exceptions;
using Truckero.Core.Interfaces.Services;
using TruckeroApp.Interfaces;
using TruckeroApp.ServiceClients.ApiHelpers;

public class PayoutAccountApiClientService : IPayoutAccountService {
    private readonly HttpClient _http;
    private readonly IAuthSessionContext _session;
    private readonly ILogger<PayoutAccountApiClientService> _logger;

    public PayoutAccountApiClientService(HttpClient http, IAuthSessionContext session, ILogger<PayoutAccountApiClientService> logger) {
        _http = http;
        _session = session;
        _logger = logger;
    }

    private string RequireAccessToken()
        => _session.AccessToken ?? throw new UnauthorizedAccessException("No access token present in session.");

    public async Task<PayoutAccountResponse?> GetPayoutAccountByIdAsync(Guid payoutAccountId) {
        // ENVELOPE ON
        // var envelope = AuthenticatedEnvelope.Create(RequireAccessToken(), _http, HttpMethod.Get, $"/api/payoutaccount/{payoutAccountId}");
        // var response = await envelope.SendAsync<HttpResponseMessage>();
        // ENVELOPE OFF
        var response = await _http.GetAsync($"/api/payoutaccount/{payoutAccountId}");

        if (response.IsSuccessStatusCode)
            return await response.Content.ReadFromJsonAsync<PayoutAccountResponse>();

        var content = await response.Content.ReadAsStringAsync();
        await HandleErrorResponse(response, content, "GetPayoutAccountById");
        return null;
    }

    public async Task<List<PayoutAccountResponse>> GetPayoutAccountsByUserIdAsync(Guid userId) {
        // ENVELOPE ON
        // var envelope = AuthenticatedEnvelope.Create(RequireAccessToken(), _http, HttpMethod.Get, $"/api/payoutaccount/user/{userId}");
        // var response = await envelope.SendAsync<HttpResponseMessage>();
        // ENVELOPE OFF
        var response = await _http.GetAsync($"/api/payoutaccount/user/{userId}");

        if (response.IsSuccessStatusCode)
            return await response.Content.ReadFromJsonAsync<List<PayoutAccountResponse>>() ?? new List<PayoutAccountResponse>();

        var content = await response.Content.ReadAsStringAsync();
        await HandleErrorResponse(response, content, "GetPayoutAccountsByUserId");
        return new List<PayoutAccountResponse>();
    }

    public async Task<PayoutAccountResponse?> GetDefaultPayoutAccountByUserIdAsync(Guid userId) {
        // ENVELOPE ON
        // var envelope = AuthenticatedEnvelope.Create(RequireAccessToken(), _http, HttpMethod.Get, $"/api/payoutaccount/user/{userId}/default");
        // var response = await envelope.SendAsync<HttpResponseMessage>();
        // ENVELOPE OFF
        var response = await _http.GetAsync($"/api/payoutaccount/user/{userId}/default");

        if (response.IsSuccessStatusCode)
            return await response.Content.ReadFromJsonAsync<PayoutAccountResponse>();

        var content = await response.Content.ReadAsStringAsync();
        await HandleErrorResponse(response, content, "GetDefaultPayoutAccountByUserId");
        return null;
    }

    public async Task<PayoutAccountResponse> AddPayoutAccountAsync(Guid userId, PayoutAccountRequest payoutAccountRequest) {
        // ENVELOPE ON
        // var envelope = AuthenticatedEnvelope.Create(RequireAccessToken(), _http, HttpMethod.Post, $"/api/payoutaccount/user/{userId}", payoutAccountRequest);
        // var response = await envelope.SendAsync<HttpResponseMessage>();
        // ENVELOPE OFF
        var response = await _http.PostAsJsonAsync($"/api/payoutaccount/user/{userId}", payoutAccountRequest);

        if (response.IsSuccessStatusCode) {
            return await response.Content.ReadFromJsonAsync<PayoutAccountResponse>()
                   ?? throw new Exception("Empty payout account response.");
        }

        var content = await response.Content.ReadAsStringAsync();
        await HandleErrorResponse(response, content, "AddPayoutAccount");
        throw new Exception("Unrecognized error creating payout account."); // Should not reach here
    }

    public async Task<PayoutAccountResponse> UpdatePayoutAccountAsync(Guid userId, Guid payoutAccountId, PayoutAccountRequest payoutAccountRequest) {
        // ENVELOPE ON
        // var envelope = AuthenticatedEnvelope.Create(RequireAccessToken(), _http, HttpMethod.Put, $"/api/payoutaccount/user/{userId}/{payoutAccountId}", payoutAccountRequest);
        // var response = await envelope.SendAsync<HttpResponseMessage>();
        // ENVELOPE OFF
        var response = await _http.PutAsJsonAsync($"/api/payoutaccount/user/{userId}/{payoutAccountId}", payoutAccountRequest);

        if (response.IsSuccessStatusCode) {
            return await response.Content.ReadFromJsonAsync<PayoutAccountResponse>()
                   ?? throw new Exception("Empty payout account response.");
        }

        var content = await response.Content.ReadAsStringAsync();
        await HandleErrorResponse(response, content, "UpdatePayoutAccount");
        throw new Exception("Unrecognized error updating payout account.");
    }

    public async Task DeletePayoutAccountAsync(Guid userId, Guid payoutAccountId) {
        // ENVELOPE ON
        // var envelope = AuthenticatedEnvelope.Create(RequireAccessToken(), _http, HttpMethod.Delete, $"/api/payoutaccount/user/{userId}/{payoutAccountId}");
        // var response = await envelope.SendAsync<HttpResponseMessage>();
        // ENVELOPE OFF
        var response = await _http.DeleteAsync($"/api/payoutaccount/user/{userId}/{payoutAccountId}");

        if (response.IsSuccessStatusCode)
            return;

        var content = await response.Content.ReadAsStringAsync();
        await HandleErrorResponse(response, content, "DeletePayoutAccount");
    }

    public async Task SetDefaultPayoutAccountAsync(Guid userId, Guid payoutAccountId) {
        // ENVELOPE ON
        // var envelope = AuthenticatedEnvelope.Create(RequireAccessToken(), _http, HttpMethod.Post, $"/api/payoutaccount/user/{userId}/setdefault/{payoutAccountId}", null);
        // var response = await envelope.SendAsync<HttpResponseMessage>();
        // ENVELOPE OFF
        var response = await _http.PostAsync($"/api/payoutaccount/user/{userId}/setdefault/{payoutAccountId}", null);

        if (response.IsSuccessStatusCode)
            return;

        var content = await response.Content.ReadAsStringAsync();
        await HandleErrorResponse(response, content, "SetDefaultPayoutAccount");
    }

    public async Task<List<PaymentMethodType>> GetAllPayoutPaymentMethodsAsync() {
        var response = await _http.GetAsync("/api/payoutaccount/types");
        if (response.IsSuccessStatusCode)
            return await response.Content.ReadFromJsonAsync<List<PaymentMethodType>>() ?? new List<PaymentMethodType>();

        var content = await response.Content.ReadAsStringAsync();
        await HandleErrorResponse(response, content, "GetAllPayoutPaymentMethods");
        return new List<PaymentMethodType>();
    }

    public async Task<List<PaymentMethodType>> GetAllPayoutPaymentMethodsByCountryCodeAsync(string countryCode) {
        var response = await _http.GetAsync($"/api/payoutaccount/types/{countryCode}");
        if (response.IsSuccessStatusCode)
            return await response.Content.ReadFromJsonAsync<List<PaymentMethodType>>() ?? new List<PaymentMethodType>();

        var content = await response.Content.ReadAsStringAsync();
        await HandleErrorResponse(response, content, "GetAllPayoutPaymentMethodsByCountryCode");
        return new List<PaymentMethodType>();
    }

    public async Task<PayoutPageReferenceDataDto> GetPayoutPageReferenceDataAsync(string countryCode) {
        var response = await _http.GetAsync($"/api/payoutaccount/reference/{countryCode}");
        if (response.IsSuccessStatusCode)
            return await response.Content.ReadFromJsonAsync<PayoutPageReferenceDataDto>() ?? new PayoutPageReferenceDataDto();

        var content = await response.Content.ReadAsStringAsync();
        await HandleErrorResponse(response, content, "GetPayoutPageReferenceData");
        return new PayoutPageReferenceDataDto();
    }

    /// <summary>
    /// Handles all API error responses for PayoutAccount methods.
    /// </summary>
    private async Task HandleErrorResponse(HttpResponseMessage response, string content, string operationName) {
        try {
            // Validation problem details (RFC 7807)
            if (response.StatusCode == HttpStatusCode.BadRequest &&
                response.Content.Headers.ContentType?.MediaType?.Contains("json") == true) {
                var validationDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
                if (validationDetails?.Errors?.Any() == true) {
                    var messages = validationDetails.Errors
                        .SelectMany(kvp => kvp.Value)
                        .Distinct()
                        .ToList();

                    throw new PayoutAccountClientValidationException("Validation failed.", messages, response.StatusCode);
                }
            }

            // Structured error response (your backend shape)
            var error = JsonSerializer.Deserialize<ErrorResponse>(content);
            if (error != null && !string.IsNullOrWhiteSpace(error.Error)) {
                // Optionally: recognize referential integrity codes or others here
                if (IsReferentialIntegrityCode(error.Code)) {
                    throw new ReferentialIntegrityClientException(error.Error, error.Code, response.StatusCode);
                }
                throw new PayoutAccountClientException(error.Error, error.Code, response.StatusCode);
            }

            // Optionally: Check for PayoutAccountResponse with Success == false (if server always returns that shape)
            var payoutResp = JsonSerializer.Deserialize<PayoutAccountResponse>(content);
            if (payoutResp != null && payoutResp.Success == false) {
                throw new PayoutAccountClientException(payoutResp.Message ?? "Operation failed.", payoutResp.ErrorCode, response.StatusCode);
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
            "PAYOUT_ACCOUNT_NOT_FOUND" => true,
            "CANNOT_DELETE_DEFAULT" => true,
            // Add others as needed
            _ => false
        };
    }
}
