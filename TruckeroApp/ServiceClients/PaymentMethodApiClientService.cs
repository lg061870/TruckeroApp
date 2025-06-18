using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Truckero.Core.DTOs.Common;
using Truckero.Core.DTOs.Billing;
using Truckero.Core.Entities;
using Truckero.Core.Exceptions;
using Truckero.Core.Interfaces.Services;
using TruckeroApp.Interfaces;
using TruckeroApp.ServiceClients.ApiHelpers;

namespace TruckeroApp.ServiceClients;

public class PaymentMethodApiClientService : IPaymentMethodService
{
    private readonly HttpClient _http;
    private readonly IAuthSessionContext _session;
    private readonly ILogger<PaymentMethodApiClientService> _logger;

    public PaymentMethodApiClientService(HttpClient http, IAuthSessionContext session, ILogger<PaymentMethodApiClientService> logger)
    {
        _http = http ?? throw new ArgumentNullException(nameof(http));
        _session = session ?? throw new ArgumentNullException(nameof(session));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    private string RequireAccessToken() => _session.AccessToken ?? throw new UnauthorizedAccessException("No access token.");

    private async Task<T> HandleResponse<T>(HttpResponseMessage response, string operationName) where T : class
    {
        if (response.IsSuccessStatusCode)
        {
            if (response.StatusCode == HttpStatusCode.NoContent || response.Content.Headers.ContentLength == 0)
            {
                return null!; // For DELETE or operations returning no content but 2xx status
            }
            var result = await response.Content.ReadFromJsonAsync<T>();
            if (result == null) // Should only happen if content was truly empty and T is not string or byte[]
            {
                 _logger.LogWarning("API response for {Operation} was successful but content deserialized to null.", operationName);
                // Depending on T, this might be acceptable or an error.
                // If T is OperationResult, null is an issue. If T is PayoutAccount?, null is fine.
            }
            return result!;
        }

        var content = await response.Content.ReadAsStringAsync();
        _logger.LogWarning("API call for {Operation} failed: {StatusCode}. Response: {Content}", operationName, response.StatusCode, content);

        try
        {
            if (response.StatusCode == HttpStatusCode.BadRequest && response.Content.Headers.ContentType?.MediaType?.Contains("application/problem+json") == true)
            {
                var validationDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
                if (validationDetails?.Errors?.Any() == true)
                {
                    throw new PaymentMethodClientValidationException($"Validation failed for {operationName}.", validationDetails.Errors.SelectMany(kvp => kvp.Value).ToList(), response.StatusCode);
                }
            }
            var opResultError = JsonSerializer.Deserialize<OperationResult>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (opResultError != null && !string.IsNullOrWhiteSpace(opResultError.Message))
            {
                throw new PaymentMethodClientException(opResultError.Message, opResultError.ErrorCode, response.StatusCode);
            }
        }
        catch (JsonException jsonEx)
        {
             _logger.LogError(jsonEx, "JSON error for {Operation} ({StatusCode}): {Content}", operationName, response.StatusCode, content);
        }
        throw new HttpRequestException($"API request for {operationName} failed. Status: {response.StatusCode}, Content: {content}", null, response.StatusCode);
    }

    public async Task<PaymentMethod?> GetPaymentMethodByIdAsync(Guid paymentMethodId, Guid userId)
    {
        var envelope = AuthenticatedEnvelope.Create(RequireAccessToken(), _http, HttpMethod.Get, $"/api/paymentmethod/{paymentMethodId}/user/{userId}");
        var response = await envelope.SendAsync<HttpResponseMessage>();
        return await HandleResponse<PaymentMethod>(response, nameof(GetPaymentMethodByIdAsync));
    }

    public async Task<List<PaymentMethod>> GetPaymentMethodsByUserIdAsync(Guid userId)
    {
        var envelope = AuthenticatedEnvelope.Create(RequireAccessToken(), _http, HttpMethod.Get, $"/api/paymentmethod/user/{userId}");
        var response = await envelope.SendAsync<HttpResponseMessage>();
        return await HandleResponse<List<PaymentMethod>>(response, nameof(GetPaymentMethodsByUserIdAsync)) ?? new List<PaymentMethod>();
    }

    public async Task<PaymentMethod?> GetDefaultPaymentMethodByUserIdAsync(Guid userId)
    {
        var envelope = AuthenticatedEnvelope.Create(RequireAccessToken(), _http, HttpMethod.Get, $"/api/paymentmethod/user/{userId}/default");
        var response = await envelope.SendAsync<HttpResponseMessage>();
        return await HandleResponse<PaymentMethod>(response, nameof(GetDefaultPaymentMethodByUserIdAsync));
    }

    public async Task<OperationResult<PaymentMethod>> AddPaymentMethodAsync(Guid userId, PaymentMethodDto paymentMethodDto)
    {
        var envelope = AuthenticatedEnvelope.Create(RequireAccessToken(), _http, HttpMethod.Post, $"/api/paymentmethod/user/{userId}", paymentMethodDto);
        var response = await envelope.SendAsync<HttpResponseMessage>();
        return await HandleResponse<OperationResult<PaymentMethod>>(response, nameof(AddPaymentMethodAsync));
    }

    public async Task<OperationResult<PaymentMethod>> UpdatePaymentMethodAsync(Guid userId, Guid paymentMethodId, PaymentMethodDto paymentMethodDto)
    {
        var envelope = AuthenticatedEnvelope.Create(RequireAccessToken(), _http, HttpMethod.Put, $"/api/paymentmethod/user/{userId}/{paymentMethodId}", paymentMethodDto);
        var response = await envelope.SendAsync<HttpResponseMessage>();
        return await HandleResponse<OperationResult<PaymentMethod>>(response, nameof(UpdatePaymentMethodAsync));
    }

    public async Task<OperationResult> DeletePaymentMethodAsync(Guid userId, Guid paymentMethodId)
    {
        var envelope = AuthenticatedEnvelope.Create(RequireAccessToken(), _http, HttpMethod.Delete, $"/api/paymentmethod/user/{userId}/{paymentMethodId}");
        var response = await envelope.SendAsync<HttpResponseMessage>();
        return await HandleResponse<OperationResult>(response, nameof(DeletePaymentMethodAsync));
    }

    public async Task<OperationResult> SetDefaultPaymentMethodAsync(Guid userId, Guid paymentMethodId)
    {
        var envelope = AuthenticatedEnvelope.Create(RequireAccessToken(), _http, HttpMethod.Post, $"/api/paymentmethod/user/{userId}/setdefault/{paymentMethodId}", null);
        var response = await envelope.SendAsync<HttpResponseMessage>();
        return await HandleResponse<OperationResult>(response, nameof(SetDefaultPaymentMethodAsync));
    }

    public async Task<List<PaymentMethodType>> GetAvailablePaymentMethodTypesAsync()
    {
        // Assuming public or AuthenticatedEnvelope handles auth if needed
        var response = await _http.GetAsync("/api/paymentmethod/types");
        return await HandleResponse<List<PaymentMethodType>>(response, nameof(GetAvailablePaymentMethodTypesAsync)) ?? new List<PaymentMethodType>();
    }
}