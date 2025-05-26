using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Truckero.Core.DTOs;
using Truckero.Core.DTOs.Auth;
using Truckero.Core.DTOs.Common;
using Truckero.Core.DTOs.Onboarding;
using Truckero.Core.Entities;
using Truckero.Core.Exceptions;
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

        var content = await response.Content.ReadAsStringAsync();

        try
        {
            // Handle server-side model validation errors (400 + ModelState)
            if (response.StatusCode == HttpStatusCode.BadRequest &&
                response.Content.Headers.ContentType?.MediaType?.Contains("json") == true)
            {
                var validationDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
                if (validationDetails?.Errors?.Any() == true)
                {
                    var messages = validationDetails.Errors
                        .SelectMany(kvp => kvp.Value)
                        .Distinct()
                        .ToList();

                    throw new OnboardingClientValidationException("Validation failed.", messages, response.StatusCode);
                }
            }

            // Handle structured onboarding failures (e.g., profile creation)

            var error = JsonSerializer.Deserialize<ErrorResponse>(content);

            if (error != null && !string.IsNullOrWhiteSpace(error.Error))
            {
                throw new OnboardingClientException(error.Error, error.Code, response.StatusCode);
            }
        }
        catch (JsonException)
        {
            // Fall through to generic failure
        }

        throw new HttpRequestException($"Unrecognized error response: {content}", null, response.StatusCode);
    }

    public async Task<OnboardingVerificationResult> VerifyIfOperationSuccessfulAsync(string email)
    {
        var result = new OnboardingVerificationResult();

        try
        {
            var user = await _http.GetFromJsonAsync<User>($"auth/user/by-email?email={Uri.EscapeDataString(email)}");
            if (user == null)
                return result;

            result.UserFound = true;
            result.UserId = user.Id;

            var profileResponse = await _http.GetAsync($"/api/Customer/{user.Id}");
            result.ProfileFound = profileResponse.IsSuccessStatusCode;

            var tokenResponse = await _http.GetAsync($"tokens/user/{user.Id}");
            result.TokenFound = tokenResponse.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[VerifyIfOperationSuccessful] Exception: {ex.Message}");
        }

        return result;
    }

    public async Task<OperationResult> SendConfirmationEmailAsync(Guid userId)
    {
        var response = await _http.PostAsync($"/onboarding/send-confirmation-email?userId={userId}", null);
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<OperationResult>();
            return result ?? OperationResult.Succeeded("Confirmation email sent.");
        }
        else
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            return OperationResult.Failed($"Failed to send confirmation email: {errorContent}");
        }
    }

    public async Task<OperationResult> ConfirmEmailAsync(string token)
    {
        var response = await _http.PostAsync($"/onboarding/confirm-email?token={Uri.EscapeDataString(token)}", null);
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<OperationResult>();
            return result ?? OperationResult.Succeeded("Email confirmed successfully.");
        }
        else
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            return OperationResult.Failed($"Failed to confirm email: {errorContent}");
        }
    }
}
