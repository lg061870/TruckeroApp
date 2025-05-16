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
            setError?.Invoke(httpEx.StatusCode switch
            {
                HttpStatusCode.BadRequest => "Invalid input provided.",
                HttpStatusCode.NotFound => "Resource not found.",
                HttpStatusCode.Unauthorized => "Unauthorized. Please log in again.",
                HttpStatusCode.InternalServerError => "Internal server error.",
                _ when httpEx.StatusCode.HasValue =>
                    $"Unexpected HTTP error: {(int)httpEx.StatusCode.Value} {httpEx.Message}",
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
