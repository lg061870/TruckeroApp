using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using Truckero.Core.Entities;
using Truckero.Core.Interfaces.Services;
using TruckeroApp.ServiceClients.ApiHelpers;
using TruckeroApp.Interfaces;

namespace TruckeroApp.ServiceClients;

public class DriverApiClientService : IDriverService
{
    private readonly HttpClient _http;
    private readonly IAuthSessionContext _session;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public DriverApiClientService(HttpClient http, IAuthSessionContext session)
    {
        _http = http;
        _session = session;
    }

    private string RequireAccessToken()
        => _session.AccessToken ?? throw new UnauthorizedAccessException("No access token present in session.");

    public async Task<DriverProfile?> GetByUserIdAsync(Guid userId)
    {
        var envelope = AuthenticatedEnvelope.Create(
            RequireAccessToken(), _http, HttpMethod.Get, $"/api/Driver/{userId}"
        );
        var response = await envelope.SendAsync<HttpResponseMessage>();
        if (!response.IsSuccessStatusCode) return null;

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<DriverProfile>(json, _jsonOptions);
    }

    public async Task<List<Truck>> GetVehiclesAsync(Guid userId)
    {
        var envelope = AuthenticatedEnvelope.Create(
            RequireAccessToken(), _http, HttpMethod.Get, $"/api/Driver/{userId}/vehicles"
        );
        var response = await envelope.SendAsync<HttpResponseMessage>();
        if (!response.IsSuccessStatusCode) return new();

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<Truck>>(json, _jsonOptions) ?? new();
    }

    public async Task AddVehicleAsync(Truck vehicle)
    {
        var envelope = AuthenticatedEnvelope.Create(
            RequireAccessToken(), _http, HttpMethod.Post, "/api/Driver/vehicles", vehicle
        );
        var response = await envelope.SendAsync<HttpResponseMessage>();
        response.EnsureSuccessStatusCode();
    }

    public async Task UpdateVehicleAsync(Truck vehicle)
    {
        var envelope = AuthenticatedEnvelope.Create(
            RequireAccessToken(), _http, HttpMethod.Put, $"/api/Driver/vehicles/{vehicle.Id}", vehicle
        );
        var response = await envelope.SendAsync<HttpResponseMessage>();
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteVehicleAsync(Guid vehicleId)
    {
        var envelope = AuthenticatedEnvelope.Create(
            RequireAccessToken(), _http, HttpMethod.Delete, $"/api/Driver/vehicles/{vehicleId}"
        );
        var response = await envelope.SendAsync<HttpResponseMessage>();
        response.EnsureSuccessStatusCode();
    }
}
