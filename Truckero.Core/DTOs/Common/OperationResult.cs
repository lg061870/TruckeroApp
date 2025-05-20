namespace Truckero.Core.DTOs.Common
{
    /// <summary>
    /// Represents the result of an operation with success status and optional message
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
        /// Creates a successful operation result
        /// </summary>
        /// <param name="message">Optional success message</param>
        /// <returns>A successful operation result</returns>
        public static OperationResult Succeeded(string? message = null)
        {
            return new OperationResult { Success = true, Message = message };
        }

        /// <summary>
        /// Creates a failed operation result
        /// </summary>
        /// <param name="message">Error message describing the failure</param>
        /// <returns>A failed operation result</returns>
        public static OperationResult Failed(string message)
        {
            return new OperationResult { Success = false, Message = message };
        }
    }
}
