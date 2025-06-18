namespace Truckero.Core.DTOs.Common;

/// <summary>
/// Represents the result of an operation with success status, optional message, and error code
/// </summary>
public class OperationResult
{
    /// <summary>
    /// Indicates whether the operation was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Optional message providing additional information about the operation result
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Optional error code providing additional information about the operation result
    /// </summary>
    public string? ErrorCode { get; set; }

    /// <summary>
    /// Creates a successful operation result
    /// </summary>
    /// <param name="message">Optional success message</param>
    /// <returns>A successful operation result</returns>
    public static OperationResult Succeeded(string? message = null) => new() { Success = true, Message = message };

    /// <summary>
    /// Creates a failed operation result
    /// </summary>
    /// <param name="message">Error message describing the failure</param>
    /// <param name="errorCode">Optional error code describing the failure</param>
    /// <returns>A failed operation result</returns>
    public static OperationResult Failed(string? message = null, string? errorCode = null) => new() { Success = false, Message = message, ErrorCode = errorCode };
}

/// <summary>
/// Represents the result of an operation with success status, optional message, error code, and data
/// </summary>
/// <typeparam name="T">The type of the data</typeparam>
public class OperationResult<T> : OperationResult
{
    /// <summary>
    /// The data associated with the operation result
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// Creates a successful operation result with data
    /// </summary>
    /// <param name="data">The data associated with the operation result</param>
    /// <param name="message">Optional success message</param>
    /// <returns>A successful operation result with data</returns>
    public static OperationResult<T> Succeeded(T data, string? message = null) => new() { Success = true, Data = data, Message = message };

    /// <summary>
    /// Creates a failed operation result
    /// </summary>
    /// <param name="message">Error message describing the failure</param>
    /// <param name="errorCode">Optional error code describing the failure</param>
    /// <returns>A failed operation result</returns>
    public new static OperationResult<T> Failed(string? message = null, string? errorCode = null) => new() { Success = false, Message = message, ErrorCode = errorCode };
}
