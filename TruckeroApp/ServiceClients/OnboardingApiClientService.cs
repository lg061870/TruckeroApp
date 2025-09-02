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
using TruckeroApp.Interfaces;
using TruckeroApp.ServiceClients.ApiHelpers;

namespace TruckeroApp.ServiceClients;

public class OnboardingApiClientService : IOnboardingService
{
    private readonly HttpClient _http;
    private readonly IAuthSessionContext _session;

    public OnboardingApiClientService(HttpClient http, IAuthSessionContext session)
    {
        _http = http;
        _session = session;
    }

    private string RequireAccessToken()
        => _session.AccessToken ?? throw new UnauthorizedAccessException("No access token present in session.");

    // UNAUTHENTICATED (open) - if you want to lock down, just use envelope and RequireAccessToken
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
        // Authenticated (should require login)
        var envelope = AuthenticatedEnvelope.Create(RequireAccessToken(), _http, HttpMethod.Get, $"/onboarding/progress?userId={userId}");
        var response = await envelope.SendAsync<HttpResponseMessage>();
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<OnboardingProgressResponse>())!;
    }

    public async Task<AuthTokenResponse> CompleteDriverOnboardingAsync(DriverProfileRequest request)
    {
        var response = await _http.PostAsJsonAsync("/onboarding/driver", request);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<AuthTokenResponse>()
                   ?? throw new Exception("Empty token response");
        }

        var content = await response.Content.ReadAsStringAsync();

        try
        {
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

            var error = JsonSerializer.Deserialize<BaseResponse>(content);

            if (error != null && !string.IsNullOrWhiteSpace(error.Message))
            {
                throw new OnboardingClientException(error.Message, error.ErrorCode, response.StatusCode);
            }
        }
        catch (JsonException)
        {
            // Fall through to generic failure
        }

        throw new HttpRequestException($"Unrecognized error response: {content}", null, response.StatusCode);
    }

    public async Task<AuthTokenResponse> CompleteCustomerOnboardingAsync(CustomerOnboardingRequest request)
    {
        //var envelope = AuthenticatedEnvelope.Create(RequireAccessToken(), _http, HttpMethod.Post, "/onboarding/customer", request);
        //var response = await envelope.SendAsync<HttpResponseMessage>();

        var response = await _http.PostAsJsonAsync("/onboarding/customer", request);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<AuthTokenResponse>()
                   ?? throw new Exception("Empty token response");
        }

        var content = await response.Content.ReadAsStringAsync();

        try
        {
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

            var error = JsonSerializer.Deserialize<BaseResponse>(content);

            if (error != null && !string.IsNullOrWhiteSpace(error.Message))
            {
                throw new OnboardingClientException(error.Message, error.ErrorCode, response.StatusCode);
            }
        }
        catch (JsonException)
        {
            // Fall through to generic failure
        }

        throw new HttpRequestException($"Unrecognized error response: {content}", null, response.StatusCode);
    }

    // Unauthenticated: this checks user state, not a protected resource
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

    // These two are likely safe as unauthenticated, but if you want to lock them, use envelope.
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
