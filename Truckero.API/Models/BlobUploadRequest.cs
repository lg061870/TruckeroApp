namespace Truckero.API.Models;

public class BlobUploadRequest
{
    public string FileName { get; set; } = null!;
    public string ContainerName { get; set; } = null!;
}
