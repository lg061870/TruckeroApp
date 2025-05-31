using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace TruckeroApp.ServiceClients.ApiHelpers;
public class AuthenticatedEnvelope
{
    private readonly HttpClient _httpClient;
    private readonly HttpRequestMessage _request;

    private AuthenticatedEnvelope(HttpClient httpClient, HttpRequestMessage request)
    {
        _httpClient = httpClient;
        _request = request;
    }

    public static AuthenticatedEnvelope Create(string accessToken, HttpClient httpClient, HttpMethod method, string endpoint, object? body = null)
    {
        var request = new HttpRequestMessage(method, endpoint);

        // Set the Authorization header if token is provided
        if (!string.IsNullOrWhiteSpace(accessToken))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        // Add JSON body if POST/PUT/PATCH and body provided
        if (body != null && (method == HttpMethod.Post || method == HttpMethod.Put || method.Method == "PATCH"))
        {
            request.Content = JsonContent.Create(body);
        }

        return new AuthenticatedEnvelope(httpClient, request);
    }

    // Send and deserialize the response as T
    public async Task<T> SendAsync<T>(JsonSerializerOptions? options = null)
    {
        var response = await _httpClient.SendAsync(_request);

        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode == HttpStatusCode.Unauthorized &&
                response.Headers.TryGetValues("X-Error-Code", out var errorCodes))
            {
                var errorCode = errorCodes.FirstOrDefault() ?? "unauthorized";

                var message = await response.Content.ReadAsStringAsync();
                throw new AuthTokenException(message, errorCode);
            }

            var fallbackMessage = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"API call failed: {response.StatusCode} — {fallbackMessage}");
        }

        if (typeof(T) == typeof(HttpResponseMessage))
            return (T)(object)response;

        var result = await response.Content.ReadFromJsonAsync<T>(options ?? new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        if (result == null)
            throw new InvalidOperationException("Empty API response");

        return result;
    }

    // Overload for void response (no content)
    public async Task SendAsync()
    {
        using var response = await _httpClient.SendAsync(_request);
        response.EnsureSuccessStatusCode();
    }
}

public class AuthTokenException : UnauthorizedAccessException
{
    public string Reason { get; }

    public AuthTokenException(string message, string reason)
        : base(message)
    {
        Reason = reason;
    }
}
