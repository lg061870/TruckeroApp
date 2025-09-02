using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Truckero.Core.DTOs.Common;
using Truckero.Core.DTOs.PaymentMethodType;
using Truckero.Core.Exceptions;
using TruckeroApp.Interfaces;

public class PaymentMethodTypeApiClientService : IPaymentMethodTypeService {
    private readonly HttpClient _http;
    private readonly IAuthSessionContext _session;
    private readonly ILogger<PaymentMethodTypeApiClientService> _logger;

    public PaymentMethodTypeApiClientService(HttpClient http, IAuthSessionContext session, ILogger<PaymentMethodTypeApiClientService> logger) {
        _http = http ?? throw new ArgumentNullException(nameof(http));
        _session = session ?? throw new ArgumentNullException(nameof(session));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    private string RequireAccessToken() =>
        _session.AccessToken ?? throw new UnauthorizedAccessException("No access token.");

    private async Task<T?> HandleResponse<T>(HttpResponseMessage response, string operationName) {
        if (response.IsSuccessStatusCode) {
            if (response.StatusCode == HttpStatusCode.NoContent)
                return default;

            return await response.Content.ReadFromJsonAsync<T>();
        }

        var content = await response.Content.ReadAsStringAsync();
        _logger.LogWarning("API call for {Operation} failed: {StatusCode}. Response: {Content}", operationName, response.StatusCode, content);

        try {
            if (response.StatusCode == HttpStatusCode.BadRequest && response.Content.Headers.ContentType?.MediaType?.Contains("json") == true) {
                var validationDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
                if (validationDetails?.Errors?.Any() == true) {
                    throw new PaymentMethodClientValidationException($"Validation failed for {operationName}.",
                        validationDetails.Errors.SelectMany(kvp => kvp.Value).ToList(), response.StatusCode);
                }
            }
            // Add further error response handling as needed
        } catch (JsonException jsonEx) {
            _logger.LogError(jsonEx, "JSON error for {Operation} ({StatusCode}): {Content}", operationName, response.StatusCode, content);
        }

        throw new HttpRequestException($"API request for {operationName} failed. Status: {response.StatusCode}, Content: {content}", null, response.StatusCode);
    }

    // ----- NEW PATTERN METHODS -----

    public async Task<PaymentMethodTypeResponse> GetAllPaymentMethodTypesAsync() {
        var response = await _http.GetAsync("/api/paymentmethodtype");
        return await HandleResponse<PaymentMethodTypeResponse>(response, nameof(GetAllPaymentMethodTypesAsync))
            ?? new PaymentMethodTypeResponse { Success = false, Message = "No response from API." };
    }

    public async Task<PaymentMethodTypeResponse> GetPaymentMethodTypesByCountryAsync(string countryCode) {
        var code = string.IsNullOrWhiteSpace(countryCode) ? "US" : countryCode.Trim().ToUpperInvariant();
        var response = await _http.GetAsync($"/api/paymentmethodtype/country/{code}");
        return await HandleResponse<PaymentMethodTypeResponse>(response, nameof(GetPaymentMethodTypesByCountryAsync))
            ?? new PaymentMethodTypeResponse { Success = false, Message = "No response from API." };
    }

    public async Task<PaymentMethodTypeResponse?> GetPaymentMethodTypeByIdAsync(Guid id) {
        var response = await _http.GetAsync($"/api/paymentmethodtype/{id}");
        return await HandleResponse<PaymentMethodTypeResponse>(response, nameof(GetPaymentMethodTypeByIdAsync));
    }

    public async Task<PaymentMethodTypeResponse> AddPaymentMethodTypeAsync(PaymentMethodTypeRequest dto) {
        var response = await _http.PostAsJsonAsync("/api/paymentmethodtype", dto);
        return await HandleResponse<PaymentMethodTypeResponse>(response, nameof(AddPaymentMethodTypeAsync))
            ?? new PaymentMethodTypeResponse { Success = false, Message = "No response from API." };
    }

    public async Task<PaymentMethodTypeResponse> UpdatePaymentMethodTypeAsync(PaymentMethodTypeRequest dto) {
        var response = await _http.PutAsJsonAsync($"/api/paymentmethodtype/{dto.Id}", dto);
        return await HandleResponse<PaymentMethodTypeResponse>(response, nameof(UpdatePaymentMethodTypeAsync))
            ?? new PaymentMethodTypeResponse { Success = false, Message = "No response from API." };
    }

    public async Task<PaymentMethodTypeResponse> DeletePaymentMethodTypeAsync(Guid id) {
        var response = await _http.DeleteAsync($"/api/paymentmethodtype/{id}");
        return await HandleResponse<PaymentMethodTypeResponse>(response, nameof(DeletePaymentMethodTypeAsync))
            ?? new PaymentMethodTypeResponse { Success = false, Message = "No response from API." };
    }
}
