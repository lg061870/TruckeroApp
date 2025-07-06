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

    public async Task<List<PaymentMethodTypeRequest>> GetAllPaymentMethodTypesAsync() {
        var response = await _http.GetAsync("/api/paymentmethodtype");
        return await HandleResponse<List<PaymentMethodTypeRequest>>(response, nameof(GetAllPaymentMethodTypesAsync)) ?? new List<PaymentMethodTypeRequest>();
    }

    public async Task<List<PaymentMethodTypeRequest>> GetPaymentMethodTypesByCountryAsync(string countryCode) {
        var code = string.IsNullOrWhiteSpace(countryCode) ? "US" : countryCode.Trim().ToUpperInvariant();
        var response = await _http.GetAsync($"/api/paymentmethodtype/country/{code}");
        return await HandleResponse<List<PaymentMethodTypeRequest>>(response, nameof(GetPaymentMethodTypesByCountryAsync)) ?? new List<PaymentMethodTypeRequest>();
    }

    public async Task<PaymentMethodTypeRequest?> GetPaymentMethodTypeByIdAsync(Guid id) {
        var response = await _http.GetAsync($"/api/paymentmethodtype/{id}");
        return await HandleResponse<PaymentMethodTypeRequest>(response, nameof(GetPaymentMethodTypeByIdAsync));
    }

    public async Task<PaymentMethodTypeRequest> AddPaymentMethodTypeAsync(PaymentMethodTypeRequest dto) {
        var response = await _http.PostAsJsonAsync("/api/paymentmethodtype", dto);
        return await HandleResponse<PaymentMethodTypeRequest>(response, nameof(AddPaymentMethodTypeAsync))!;
    }

    public async Task UpdatePaymentMethodTypeAsync(PaymentMethodTypeRequest dto) {
        var response = await _http.PutAsJsonAsync($"/api/paymentmethodtype/{dto.Id}", dto);
        await HandleResponse<object>(response, nameof(UpdatePaymentMethodTypeAsync));
    }

    public async Task DeletePaymentMethodTypeAsync(Guid id) {
        var response = await _http.DeleteAsync($"/api/paymentmethodtype/{id}");
        await HandleResponse<object>(response, nameof(DeletePaymentMethodTypeAsync));
    }
}
