using Microsoft.Extensions.Logging;
using Truckero.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace TruckeroApp.Services.Media
{
    /// <summary>
    /// Mock implementation of IBlobStorageService for development and testing
    /// </summary>
    public class MockBlobStorageService : IBlobStorageService
    {
        private readonly ILogger<MockBlobStorageService> _logger;
        private readonly Dictionary<string, byte[]> _mockStorage = new();
        private readonly Dictionary<string, string> _contentTypes = new();

        public MockBlobStorageService(ILogger<MockBlobStorageService> logger)
        {
            _logger = logger;
        }

        /// <inheritdoc/>
        public Task<string> UploadAsync(Stream fileStream, string fileName, string containerName, string contentType)
        {
            _logger.LogInformation("MockBlobStorage: Uploading blob {FileName} to container {ContainerName}", fileName, containerName);
            
            using var memoryStream = new MemoryStream();
            fileStream.CopyTo(memoryStream);
            
            var blobId = $"{containerName}/{fileName}";
            _mockStorage[blobId] = memoryStream.ToArray();
            _contentTypes[blobId] = contentType;
            
            var mockUrl = $"https://truckero-storage-dev.blob.core.windows.net/{containerName}/{fileName}";
            return Task.FromResult(mockUrl);
        }

        /// <inheritdoc/>
        public Task<Stream?> DownloadAsync(string blobName, string containerName)
        {
            var blobId = $"{containerName}/{blobName}";
            _logger.LogInformation("MockBlobStorage: Downloading blob {BlobId}", blobId);
            
            if (_mockStorage.TryGetValue(blobId, out var data))
            {
                return Task.FromResult<Stream?>(new MemoryStream(data));
            }
            
            return Task.FromResult<Stream?>(null);
        }

        /// <inheritdoc/>
        public Task DeleteAsync(string blobName, string containerName)
        {
            var blobId = $"{containerName}/{blobName}";
            _logger.LogInformation("MockBlobStorage: Deleting blob {BlobId}", blobId);
            
            _mockStorage.Remove(blobId);
            _contentTypes.Remove(blobId);
            
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task<string> GetBlobUrl(string blobName, string containerName)
        {
            var blobId = $"{containerName}/{blobName}";
            _logger.LogInformation("MockBlobStorage: Getting URL for {BlobId}", blobId);
            
            var mockUrl = $"https://truckero-storage-dev.blob.core.windows.net/{containerName}/{blobName}";
            return Task.FromResult(mockUrl);
        }

        /// <inheritdoc/>
        public Task<string> GenerateUploadSasUrl(string blobName, string containerName, TimeSpan expiry)
        {
            var blobId = $"{containerName}/{blobName}";
            _logger.LogInformation("MockBlobStorage: Generating upload SAS URL for {BlobId}", blobId);
            
            var expiryTime = DateTime.UtcNow.Add(expiry);
            var mockUrl = $"https://truckero-storage-dev.blob.core.windows.net/{containerName}/{blobName}?sv=2022-11-02&ss=b&srt=sco&sp=rwdlacu&se={expiryTime:yyyy-MM-ddTHH:mm:ssZ}&st={DateTime.UtcNow:yyyy-MM-ddTHH:mm:ssZ}&spr=https&sig=mock_sas_token";
            
            return Task.FromResult(mockUrl);
        }

        /// <inheritdoc/>
        public Task<string> GenerateDownloadSasUrl(string blobName, string containerName, TimeSpan expiry)
        {
            var blobId = $"{containerName}/{blobName}";
            _logger.LogInformation("MockBlobStorage: Generating download SAS URL for {BlobId}", blobId);
            
            var expiryTime = DateTime.UtcNow.Add(expiry);
            var mockUrl = $"https://truckero-storage-dev.blob.core.windows.net/{containerName}/{blobName}?sv=2022-11-02&ss=b&srt=sco&sp=r&se={expiryTime:yyyy-MM-ddTHH:mm:ssZ}&st={DateTime.UtcNow:yyyy-MM-ddTHH:mm:ssZ}&spr=https&sig=mock_sas_token";
            
            return Task.FromResult(mockUrl);
        }
    }
}
