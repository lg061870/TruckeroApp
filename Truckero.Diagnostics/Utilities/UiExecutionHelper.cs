using System.Net;

namespace Truckero.Diagnostics.Utilities;

public static class UiExecutionHelper
{
    public static async Task SafeExecuteAsync(
        Func<Task> action,
        Action<string>? setError,
        Action<string>? setSuccess,
        Action<bool> setLoading,
        string? successMessage = null,
        string defaultErrorMessage = "An unexpected error occurred.")
    {
        setError?.Invoke(null!);
        setSuccess?.Invoke(null!);
        setLoading(true);

        try
        {
            await action();

            if (!string.IsNullOrWhiteSpace(successMessage))
                setSuccess?.Invoke(successMessage);
        }
        catch (HttpRequestException httpEx)
        {
            // First check if we have a detailed error message from the server
            if (!string.IsNullOrWhiteSpace(httpEx.Message) && 
                !httpEx.Message.StartsWith("Error ") && 
                !httpEx.Message.StartsWith("Response status code does not indicate success"))
            {
                // Use the detailed error message from the server
                setError?.Invoke(httpEx.Message);
                return;
            }

            // Fall back to generic messages based on status code
            setError?.Invoke(httpEx.StatusCode switch
            {
                HttpStatusCode.BadRequest => "Invalid input provided.",
                HttpStatusCode.NotFound => "Resource not found.",
                HttpStatusCode.Unauthorized => "Unauthorized. Please log in again.",
                HttpStatusCode.Conflict => "This record already exists.",
                HttpStatusCode.InternalServerError => "Internal server error.",
                _ when httpEx.StatusCode.HasValue =>
                    $"Unexpected HTTP error: {(int)httpEx.StatusCode.Value}",
                _ => "Unexpected HTTP error occurred."
            });
        }
        catch (Exception ex)
        {
            setError?.Invoke($"{defaultErrorMessage} {ex.Message}");
        }
        finally
        {
            setLoading(false);
        }
    }
}
