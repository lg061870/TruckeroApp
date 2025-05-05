using System.Text.Json;
using Truckero.Core.Interfaces;

namespace TruckeroApp.ServiceClients;

public class PaymentApiClientService : IPaymentService
{
    private readonly HttpClient _http;

    public PaymentApiClientService(HttpClient http)
    {
        _http = http;
        Console.WriteLine($"[API Client] Hitting: {_http.BaseAddress}");
    }

    public async Task<List<PaymentMethodType>> GetAllPaymentMethods(string countryCode)
    {
        const int maxRetries = 5;
        const int delayMs = 1000;

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                var response = await _http.GetAsync($"/api/PaymentMethods/GetAllPaymentMethods?countryCode={countryCode}");

                response.EnsureSuccessStatusCode(); // throws if not 2xx
                var json = await response.Content.ReadAsStringAsync();

                return JsonSerializer.Deserialize<List<PaymentMethodType>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<PaymentMethodType>();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Attempt {attempt} failed: {ex.Message}");

                if (attempt == maxRetries)
                    throw;

                await Task.Delay(delayMs);
            }
        }

        return new List<PaymentMethodType>(); // fallback
    }
    public async Task<List<PaymentMethodType>> GetAllPayoutMethods(string countryCode)
    {
        var response = await _http.GetAsync($"/api/PaymentMethods/GetAllPayoutMethods?countryCode={countryCode}");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<List<PaymentMethodType>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? new List<PaymentMethodType>();
    }

}
