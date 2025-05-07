using System.Net.Http.Json;
using System.Text.Json;
using Truckero.Core.DTOs;
using Truckero.Core.Entities;
using Truckero.Core.Interfaces.Services;

namespace TruckeroApp.ServiceClients;

public class CustomerApiClientService : ICustomerService
{
    private readonly HttpClient _http;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public CustomerApiClientService(HttpClient http)
    {
        _http = http;
        Console.WriteLine($"[API Client] Initialized with base address: {_http.BaseAddress}");
    }

    public async Task<CustomerProfile?> GetByUserIdAsync(Guid userId)
    {
        var response = await _http.GetAsync($"/api/Customer/{userId}");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<CustomerProfile>(json, _jsonOptions);
    }

    public async Task CreateAsync(CustomerProfileRequest request, Guid userId)
    {
        var response = await _http.PostAsJsonAsync($"/api/Customer?userId={userId}", request);
        response.EnsureSuccessStatusCode();
    }
}
