using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.Extensions.Configuration;
using Truckero.Core.Interfaces;

namespace Truckero.Infrastructure.Storage;

public class AzureBlobStorageService : IBlobStorageService
{
    private readonly BlobServiceClient _blobServiceClient;

    public AzureBlobStorageService(IConfiguration config)
    {
        _blobServiceClient = new BlobServiceClient(
            config.GetConnectionString("AzureBlobStorage"));
    }

    public async Task<string> UploadAsync(Stream fileStream, string fileName, string containerName, string contentType)
    {
        var container = _blobServiceClient.GetBlobContainerClient(containerName);
        await container.CreateIfNotExistsAsync();
        var blobClient = container.GetBlobClient(fileName);

        var headers = new BlobHttpHeaders { ContentType = contentType };
        await blobClient.UploadAsync(fileStream, headers);

        return blobClient.Uri.ToString();
    }

    public async Task<Stream?> DownloadAsync(string blobName, string containerName)
    {
        var container = _blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = container.GetBlobClient(blobName);

        if (await blobClient.ExistsAsync())
            return await blobClient.OpenReadAsync();

        return null;
    }

    public async Task DeleteAsync(string blobName, string containerName)
    {
        var container = _blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = container.GetBlobClient(blobName);
        await blobClient.DeleteIfExistsAsync();
    }

    public async Task<string> GetBlobUrl(string blobName, string containerName)
    {
        var container = _blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = container.GetBlobClient(blobName);
        return await Task.FromResult(blobClient.Uri.ToString());
    }

    public async Task<string> GenerateDownloadSasUrl(string blobName, string containerName, TimeSpan expiry)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(blobName);

        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = containerName,
            BlobName = blobName,
            Resource = "b",
            ExpiresOn = DateTimeOffset.UtcNow.Add(expiry)
        };
        sasBuilder.SetPermissions(BlobSasPermissions.Read);

        var sharedKey = new StorageSharedKeyCredential(
            _blobServiceClient.AccountName,
            Environment.GetEnvironmentVariable("AZURE_STORAGE_KEY")!);

        var sasToken = sasBuilder.ToSasQueryParameters(sharedKey).ToString();
        var uri = new Uri($"{blobClient.Uri}?{sasToken}");
        return await Task.FromResult(uri.ToString());
    }

    public async Task<string> GenerateUploadSasUrl(string blobName, string containerName, TimeSpan expiry)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(blobName);

        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = containerName,
            BlobName = blobName,
            Resource = "b",
            ExpiresOn = DateTimeOffset.UtcNow.Add(expiry)
        };
        sasBuilder.SetPermissions(BlobSasPermissions.Write | BlobSasPermissions.Create);

        var sharedKey = new StorageSharedKeyCredential(
            _blobServiceClient.AccountName,
            Environment.GetEnvironmentVariable("AZURE_STORAGE_KEY")!);

        var sasToken = sasBuilder.ToSasQueryParameters(sharedKey).ToString();
        var uri = new Uri($"{blobClient.Uri}?{sasToken}");
        return await Task.FromResult(uri.ToString());
    }
}
