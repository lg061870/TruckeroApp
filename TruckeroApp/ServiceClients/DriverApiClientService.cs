using System.Net.Http.Json;
using System.Text.Json;
using Truckero.Core.Entities;
using Truckero.Core.Interfaces;
using Truckero.Core.Interfaces.Services;

namespace TruckeroApp.ServiceClients;

public class DriverApiClientService : IDriverService
{
    private readonly HttpClient _http;

    public DriverApiClientService(HttpClient http)
    {
        _http = http;
        Console.WriteLine($"[API Client] Driver service using: {_http.BaseAddress}");
    }

    public async Task<DriverProfile?> GetByUserIdAsync(Guid userId)
    {
        var response = await _http.GetAsync($"/api/Driver/{userId}");
        if (!response.IsSuccessStatusCode) return null;

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<DriverProfile>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    public async Task<List<Vehicle>> GetVehiclesAsync(Guid userId)
    {
        var response = await _http.GetAsync($"/api/Driver/{userId}/vehicles");
        if (!response.IsSuccessStatusCode) return new();

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<Vehicle>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? new();
    }

    public async Task AddVehicleAsync(Vehicle vehicle)
    {
        var response = await _http.PostAsJsonAsync("/api/Driver/vehicles", vehicle);
        response.EnsureSuccessStatusCode();
    }

    public async Task UpdateVehicleAsync(Vehicle vehicle)
    {
        var response = await _http.PutAsJsonAsync($"/api/Driver/vehicles/{vehicle.Id}", vehicle);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteVehicleAsync(Guid vehicleId)
    {
        var response = await _http.DeleteAsync($"/api/Driver/vehicles/{vehicleId}");
        response.EnsureSuccessStatusCode();
    }
}
