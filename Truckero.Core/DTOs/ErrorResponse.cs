namespace Truckero.Core.DTOs
{
    /// <summary>
    /// Represents an error response from the API
    /// </summary>
    public class ErrorResponse
    {
        /// <summary>
        /// The error message
        /// </summary>
        public string? Error { get; set; }
        
        /// <summary>
        /// Optional error code for specific error handling
        /// </summary>
        public string? Code { get; set; }
    }
}
