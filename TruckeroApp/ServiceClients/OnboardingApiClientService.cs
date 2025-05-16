using System.Net.Http.Json;
using Truckero.Core.DTOs;
using Truckero.Core.DTOs.Auth;
using Truckero.Core.DTOs.Onboarding;
using Truckero.Core.Interfaces.Services;

namespace TruckeroApp.ServiceClients;

public class OnboardingApiClientService : IOnboardingService
{
    private readonly HttpClient _http;

    public OnboardingApiClientService(HttpClient http)
    {
        _http = http;
    }

    public async Task StartAsync(StartOnboardingRequest request, Guid userId)
    {
        var response = await _http.PostAsJsonAsync($"/onboarding/start?userId={userId}", request);
        response.EnsureSuccessStatusCode();
    }

    public async Task<bool> VerifyCodeAsync(VerifyCodeRequest request, Guid userId)
    {
        var response = await _http.PostAsJsonAsync("/onboarding/verify", request);
        return response.IsSuccessStatusCode;
    }

    public async Task<OnboardingProgressResponse> GetProgressAsync(Guid userId)
    {
        var response = await _http.GetAsync($"/onboarding/progress?userId={userId}");
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<OnboardingProgressResponse>())!;
    }

    public async Task CompleteCustomerOnboardingAsync(CustomerProfileRequest request, Guid userId)
    {
        var response = await _http.PostAsJsonAsync($"/onboarding/customer?userId={userId}", request);
        response.EnsureSuccessStatusCode();
    }

    public async Task CompleteDriverOnboardingAsync(DriverProfileRequest request, Guid userId)
    {
        var response = await _http.PostAsJsonAsync($"/onboarding/driver?userId={userId}", request);
        response.EnsureSuccessStatusCode();
    }

    public async Task CompleteCustomerOnboarding(CustomerOnboardingRequest request)
    {
        await _http.PostAsJsonAsync("/onboarding/customer", request);
    }

}
