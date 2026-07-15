using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Truckero.Core.DTOs;
using Truckero.Core.DTOs.Common;

namespace TruckeroApp.ServiceClients.ApiHelpers;

public abstract class ApiClientBase
{
    protected HttpClient Http { get; }
    protected JsonSerializerOptions JsonOptions { get; } = new()
    {
        PropertyNameCaseInsensitive = true
    };

    protected ApiClientBase(HttpClient http)
    {
        Http = http ?? throw new ArgumentNullException(nameof(http));
    }

    protected async Task<T> GetRequiredAsync<T>(string path, CancellationToken cancellationToken = default)
    {
        using var response = await Http.GetAsync(path, cancellationToken);
        return await ReadRequiredAsync<T>(response, $"GET {path}", cancellationToken);
    }

    protected async Task<T> PostRequiredAsync<T>(string path, object body, CancellationToken cancellationToken = default)
    {
        using var response = await Http.PostAsJsonAsync(path, body, JsonOptions, cancellationToken);
        return await ReadRequiredAsync<T>(response, $"POST {path}", cancellationToken);
    }

    protected async Task<T> PutRequiredAsync<T>(string path, object body, CancellationToken cancellationToken = default)
    {
        using var response = await Http.PutAsJsonAsync(path, body, JsonOptions, cancellationToken);
        return await ReadRequiredAsync<T>(response, $"PUT {path}", cancellationToken);
    }

    protected async Task<T?> DeleteOptionalAsync<T>(string path, CancellationToken cancellationToken = default)
    {
        using var response = await Http.DeleteAsync(path, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            await ThrowApiExceptionAsync(response, $"DELETE {path}", cancellationToken);
        }

        if (response.StatusCode == HttpStatusCode.NoContent || response.Content.Headers.ContentLength is 0)
        {
            return default;
        }

        return await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken);
    }

    protected async Task EnsureSuccessAsync(HttpResponseMessage response, string operation, CancellationToken cancellationToken = default)
    {
        if (!response.IsSuccessStatusCode)
        {
            await ThrowApiExceptionAsync(response, operation, cancellationToken);
        }
    }

    private async Task<T> ReadRequiredAsync<T>(HttpResponseMessage response, string operation, CancellationToken cancellationToken)
    {
        if (!response.IsSuccessStatusCode)
        {
            await ThrowApiExceptionAsync(response, operation, cancellationToken);
        }

        var payload = await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken);
        if (payload is null)
        {
            throw new InvalidOperationException($"Response body was empty for {operation}.");
        }

        return payload;
    }

    protected async Task ThrowApiExceptionAsync(HttpResponseMessage response, string operation, CancellationToken cancellationToken)
    {
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        var statusCode = response.StatusCode;
        var mediaType = response.Content.Headers.ContentType?.MediaType;

        if (!string.IsNullOrWhiteSpace(mediaType) &&
            mediaType.Contains("json", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                var validation = JsonSerializer.Deserialize<ValidationProblemDetails>(body, JsonOptions);
                if (validation?.Errors is { Count: > 0 })
                {
                    throw new ApiClientException(
                        $"Validation failed for {operation}.",
                        statusCode,
                        validationErrors: validation.Errors,
                        responseBody: body);
                }
            }
            catch (JsonException)
            {
                // Ignore parse failures and continue with fallback parsing.
            }

            try
            {
                var envelope = JsonSerializer.Deserialize<BaseResponse>(body, JsonOptions);
                if (envelope is not null &&
                    (!string.IsNullOrWhiteSpace(envelope.ErrorCode) || !string.IsNullOrWhiteSpace(envelope.Message)))
                {
                    var message = string.IsNullOrWhiteSpace(envelope.Message)
                        ? $"API call failed for {operation}."
                        : $"API call failed for {operation}: {envelope.Message}";

                    throw new ApiClientException(
                        message,
                        statusCode,
                        envelope.ErrorCode,
                        responseBody: body);
                }
            }
            catch (JsonException)
            {
                // Ignore parse failures and continue with generic exception.
            }
        }

        throw new ApiClientException(
            $"API call failed for {operation}. Status: {(int)statusCode} ({statusCode}). Body: {body}",
            statusCode,
            responseBody: body);
    }
}

public sealed class ApiClientException : HttpRequestException
{
    public HttpStatusCode Status { get; }
    public string? ErrorCode { get; }
    public IReadOnlyDictionary<string, string[]>? ValidationErrors { get; }
    public string? ResponseBody { get; }

    public ApiClientException(
        string message,
        HttpStatusCode status,
        string? errorCode = null,
        IReadOnlyDictionary<string, string[]>? validationErrors = null,
        string? responseBody = null)
        : base(message, null, status)
    {
        Status = status;
        ErrorCode = errorCode;
        ValidationErrors = validationErrors;
        ResponseBody = responseBody;
    }
}
