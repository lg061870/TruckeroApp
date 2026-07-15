namespace Truckero.Core.DTOs.CustomerFlow
{
    public class RequestMoreDriversResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? ErrorCode { get; set; }
    }
}