using System.Net.Http.Json;
using Truckero.Core.DTOs;
using Truckero.Core.DTOs.Auth;
using Truckero.Core.DTOs.Common;
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



    public async Task<OperationResult> CompleteDriverOnboardingAsync(DriverProfileRequest request, Guid userId)
    {
        try
        {
            var response = await _http.PostAsJsonAsync($"/onboarding/driver?userId={userId}", request);
            
            if (response.IsSuccessStatusCode)
            {
                // If successful, try to read the OperationResult from the response
                var result = await response.Content.ReadFromJsonAsync<OperationResult>();
                return result ?? OperationResult.Succeeded("Driver onboarding completed successfully");
            }
            else
            {
                // Try to extract error information from the response
                try
                {
                    var errorResponse = await response.Content.ReadFromJsonAsync<ErrorResponse>();
                    if (errorResponse != null && !string.IsNullOrEmpty(errorResponse.Error))
                    {
                        return OperationResult.Failed(errorResponse.Error);
                    }
                }
                catch
                {
                    // If we can't deserialize the error, continue to the fallback
                }
                
                // Fallback: try to read raw content
                var errorContent = await response.Content.ReadAsStringAsync();
                return OperationResult.Failed($"Error {(int)response.StatusCode}: {errorContent}");
            }
        }
        catch (Exception ex)
        {
            return OperationResult.Failed($"Failed to complete driver onboarding: {ex.Message}");
        }
    }

    public async Task<AuthTokenResponse> CompleteCustomerOnboardingAsync(CustomerOnboardingRequest request)
    {
        var response = await _http.PostAsJsonAsync("/onboarding/customer", request);
        
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<AuthTokenResponse>()
                   ?? throw new Exception("Empty token response");
        }
        else
        {
            // Try to extract error information from the response
            var errorContent = await response.Content.ReadAsStringAsync();
            
            // Try to deserialize the error response
            try 
            {
                var errorResponse = await response.Content.ReadFromJsonAsync<ErrorResponse>();
                if (errorResponse != null && !string.IsNullOrEmpty(errorResponse.Error))
                {
                    throw new HttpRequestException(errorResponse.Error, null, response.StatusCode);
                }
            }
            catch
            {
                // If we can't deserialize the error, just include the raw content in the exception
            }
            
            // If we couldn't extract a specific error message, throw a generic exception with the status code
            throw new HttpRequestException($"Error {(int)response.StatusCode}: {errorContent}", 
                null, response.StatusCode);
        }
    }


}
