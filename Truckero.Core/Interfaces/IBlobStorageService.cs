namespace Truckero.Core.Interfaces;

public interface IBlobStorageService
{
    Task<string> UploadAsync(Stream fileStream, string fileName, string containerName, string contentType);
    Task<Stream?> DownloadAsync(string blobName, string containerName);
    Task DeleteAsync(string blobName, string containerName);
    Task<string> GetBlobUrl(string blobName, string containerName);
    Task<string> GenerateUploadSasUrl(string blobName, string containerName, TimeSpan expiry);
    Task<string> GenerateDownloadSasUrl(string blobName, string containerName, TimeSpan expiry);
}


