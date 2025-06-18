using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Truckero.Core.DTOs;
using Truckero.Core.DTOs.Common;
using Truckero.Core.DTOs.Onboarding;
using Truckero.Core.DTOs.Trucks;
using Truckero.Core.Entities;
using Truckero.Core.Exceptions;
using Truckero.Core.Interfaces.Services;
using TruckeroApp.Interfaces;
using TruckeroApp.ServiceClients.ApiHelpers;

public class TruckApiClientService : ITruckService {
    private readonly HttpClient _http;
    private readonly IAuthSessionContext _session;

    public TruckApiClientService(HttpClient http, IAuthSessionContext session) {
        _http = http;
        _session = session;
    }

    private string RequireAccessToken()
        => _session.AccessToken ?? throw new UnauthorizedAccessException("No access token present in session.");

    public async Task<List<Truck>> GetDriverTrucksAsync(Guid userId) {
        var envelope = AuthenticatedEnvelope.Create(
            RequireAccessToken(), _http, HttpMethod.Get, $"/api/truck/driver/{userId}");
        return await envelope.SendAsync<List<Truck>>();
    }

    public async Task<TruckResponseDto> AddDriverTruckAsync(Guid userId, TruckRequestDto truck) {
        var response = await _http.PostAsJsonAsync($"/api/truck/driver/{userId}", truck);

        if (response.IsSuccessStatusCode) {
            return await response.Content.ReadFromJsonAsync<TruckResponseDto>()
                   ?? throw new Exception("Empty truck response");
        }

        var content = await response.Content.ReadAsStringAsync();

        try {
            // Handle validation errors
            if (response.StatusCode == HttpStatusCode.BadRequest &&
                response.Content.Headers.ContentType?.MediaType?.Contains("json") == true) {
                var validationDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
                if (validationDetails?.Errors?.Any() == true) {
                    var messages = validationDetails.Errors
                        .SelectMany(kvp => kvp.Value)
                        .Distinct()
                        .ToList();

                    throw new TruckClientValidationException("Validation failed.", messages, response.StatusCode);
                }
            }

            // Handle structured error response
            var error = JsonSerializer.Deserialize<ErrorResponse>(content);

            if (error != null && !string.IsNullOrWhiteSpace(error.Error)) {
                // Recognize referential integrity error codes (expand list as needed)
                if (IsReferentialIntegrityCode(error.Code)) {
                    throw new ReferentialIntegrityClientException(
                        error.Error, error.Code, response.StatusCode);
                }
                throw new TruckClientException(error.Error, error.Code, response.StatusCode);
            }
        } catch (JsonException) {
            // Fall through to generic failure
        }

        throw new HttpRequestException($"Unrecognized error response: {content}", null, response.StatusCode);
    }

    /// <summary>
    /// Helper to check if the error code is referential integrity-related.
    /// Update this list as you add more codes.
    /// </summary>
    private bool IsReferentialIntegrityCode(string? code) {
        if (string.IsNullOrWhiteSpace(code))
            return false;

        return code switch {
            "MISSING_DRIVER_PROFILE" => true,
            "MISSING_TRUCK_TYPE" => true,
            "MISSING_TRUCK_MODEL" => true,
            "DUPLICATE_LICENSE_PLATE" => true,
            "FOREIGN_KEY_NOT_FOUND" => true,
            // Add more codes here...
            _ => false
        };
    }

    public async Task<TruckResponseDto> UpdateDriverTruckAsync(Guid userId, Guid truckId, TruckRequestDto truck) {
        var envelope = AuthenticatedEnvelope.Create(
            RequireAccessToken(), _http, HttpMethod.Put, $"/api/truck/driver/{userId}/{truckId}", truck);
        var response = await envelope.SendAsync<HttpResponseMessage>();

        if (response.IsSuccessStatusCode) {
            return await response.Content.ReadFromJsonAsync<TruckResponseDto>()
                   ?? throw new Exception("Empty truck response");
        }

        var content = await response.Content.ReadAsStringAsync();

        try {
            if (response.StatusCode == HttpStatusCode.BadRequest &&
                response.Content.Headers.ContentType?.MediaType?.Contains("json") == true) {
                var validationDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
                if (validationDetails?.Errors?.Any() == true) {
                    var messages = validationDetails.Errors
                        .SelectMany(kvp => kvp.Value)
                        .Distinct()
                        .ToList();

                    throw new TruckClientValidationException("Validation failed.", messages, response.StatusCode);
                }
            }

            var error = JsonSerializer.Deserialize<ErrorResponse>(content);

            if (error != null && !string.IsNullOrWhiteSpace(error.Error)) {
                throw new TruckClientException(error.Error, error.Code, response.StatusCode);
            }
        } catch (JsonException) {
            // Fall through to generic failure
        }

        throw new HttpRequestException($"Unrecognized error response: {content}", null, response.StatusCode);
    }

    public async Task<TruckResponseDto> DeleteDriverTruckAsync(Guid userId, Guid truckId) {
        var envelope = AuthenticatedEnvelope.Create(
            RequireAccessToken(), _http, HttpMethod.Delete, $"/api/truck/driver/{userId}/{truckId}");
        var response = await envelope.SendAsync<HttpResponseMessage>();

        if (response.IsSuccessStatusCode) {
            return await response.Content.ReadFromJsonAsync<TruckResponseDto>()
                   ?? throw new Exception("Empty truck response");
        }

        var content = await response.Content.ReadAsStringAsync();

        try {
            if (response.StatusCode == HttpStatusCode.BadRequest &&
                response.Content.Headers.ContentType?.MediaType?.Contains("json") == true) {
                var validationDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
                if (validationDetails?.Errors?.Any() == true) {
                    var messages = validationDetails.Errors
                        .SelectMany(kvp => kvp.Value)
                        .Distinct()
                        .ToList();

                    throw new TruckClientValidationException("Validation failed.", messages, response.StatusCode);
                }
            }

            var error = JsonSerializer.Deserialize<ErrorResponse>(content);

            if (error != null && !string.IsNullOrWhiteSpace(error.Error)) {
                throw new TruckClientException(error.Error, error.Code, response.StatusCode);
            }
        } catch (JsonException) {
            // Fall through to generic failure
        }

        throw new HttpRequestException($"Unrecognized error response: {content}", null, response.StatusCode);
    }

    public async Task<IEnumerable<Truck>> GetTrucksForDriverAsync(Guid driverProfileId)
        => await _http.GetFromJsonAsync<List<Truck>>($"/api/truck/driver/{driverProfileId}") ?? new List<Truck>();

    public async Task<List<TruckMake>> GetTruckMakesAsync()
        => await _http.GetFromJsonAsync<List<TruckMake>>("/api/truck/makes") ?? new List<TruckMake>();

    public async Task<List<TruckModel>> GetTruckModelsAsync(Guid? makeId = null)
        => await _http.GetFromJsonAsync<List<TruckModel>>($"/api/truck/models{(makeId != null ? $"?makeId={makeId}" : "")}") ?? new List<TruckModel>();

    public async Task<List<TruckCategory>> GetTruckCategoriesAsync()
        => await _http.GetFromJsonAsync<List<TruckCategory>>("/api/truck/categories") ?? new List<TruckCategory>();

    public async Task<List<BedType>> GetBedTypesAsync()
        => await _http.GetFromJsonAsync<List<BedType>>("/api/truck/bedtypes") ?? new List<BedType>();

    public async Task<List<UseTag>> GetUseTagsAsync()
        => await _http.GetFromJsonAsync<List<UseTag>>("/api/truck/usetags") ?? new List<UseTag>();

    public async Task<List<TruckType>> GetTruckTypesAsync()
        => await _http.GetFromJsonAsync<List<TruckType>>("/api/truck/types") ?? new List<TruckType>();

    public async Task<TruckPageReferenceDataDto> GetTruckPageDataAsync()
    {
        try
        {
            var result = await _http.GetFromJsonAsync<TruckPageReferenceDataDto>("/api/TruckeroViewProvider/truck-page-data");
            if (result == null)
            {
                // Optionally log or throw a more descriptive exception
                throw new Exception("Received null TruckPageReferenceDataDto from API.");
            }
            return result;
        }
        catch (HttpRequestException ex)
        {
            // Log details about the HTTP request failure
            Console.WriteLine($"HTTP request failed: {ex.Message}");
            throw new Exception("Failed to fetch truck page data from the server. See inner exception for details.", ex);
        }
        catch (NotSupportedException ex)
        {
            // The content type is not valid
            Console.WriteLine($"The content type is not supported: {ex.Message}");
            throw new Exception("Invalid content type received from the server.", ex);
        }
        catch (JsonException ex)
        {
            // Invalid JSON
            Console.WriteLine($"Invalid JSON: {ex.Message}");
            throw new Exception("Failed to parse truck page data from the server.", ex);
        }
        catch (Exception ex)
        {
            // Catch-all for any other exceptions
            Console.WriteLine($"Unexpected error in GetTruckPageDataAsync: {ex}");
            throw;
        }
    }
}