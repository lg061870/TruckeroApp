namespace Truckero.Core.DTOs.Common;

public class ValidationProblemDetails
{
    public Dictionary<string, string[]> Errors { get; set; } = new();
}