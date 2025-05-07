using System.Text.Json;
using Truckero.Core.Entities;
using Truckero.Core.Interfaces.Services;

namespace TruckeroApp.ServiceClients;

public class UserApiClientService : IUserService
{
    private readonly HttpClient _http;

    public UserApiClientService(HttpClient http)
    {
        _http = http;
        Console.WriteLine($"[User API] Using base URL: {_http.BaseAddress}");
    }

    public async Task<User?> GetByIdAsync(Guid userId)
    {
        var response = await _http.GetAsync($"/api/User/{userId}");
        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"[UserApi] Failed to get user by ID: {response.StatusCode}");
            return null;
        }

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<User>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        var response = await _http.GetAsync($"/api/User/by-email?email={Uri.EscapeDataString(email)}");
        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"[UserApi] Failed to get user by email: {response.StatusCode}");
            return null;
        }

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<User>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }
}
