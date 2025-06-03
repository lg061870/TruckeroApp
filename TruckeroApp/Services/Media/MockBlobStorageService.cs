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

        public MockBlobStorageService(ILogger<MockBlobStorageService> logger)
        {
            _logger = logger;
        }

        /// <inheritdoc/>
        public Task<string> UploadAsync(Stream fileStream, string fileName, string containerName, string contentType, string userId)
        {
            try
            {
                var relativePath = Path.Combine("images", userId, "license");
                var fullDirectory = Path.Combine(FileSystem.AppDataDirectory, relativePath);
                var fullPath = Path.Combine(fullDirectory, fileName);

                _logger.LogInformation("MockBlobStorage: Saving file to {FullPath}", fullPath);

                Directory.CreateDirectory(fullDirectory);

                using var fileStreamOut = File.Create(fullPath);
                fileStream.CopyTo(fileStreamOut);

                var localUrl = $"file://{fullPath}";
                return Task.FromResult(localUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MockBlobStorage: Failed to save file {FileName}", fileName);
                throw;
            }
        }

        /// <inheritdoc/>
        public Task<Stream?> DownloadAsync(string blobName, string containerName, string userId)
        {
            var path = Path.Combine(FileSystem.AppDataDirectory, "images", userId, "license", blobName);
            _logger.LogInformation("MockBlobStorage: Downloading file from {Path}", path);

            if (!File.Exists(path))
                return Task.FromResult<Stream?>(null);

            var stream = File.OpenRead(path);
            return Task.FromResult<Stream?>(stream);
        }

        /// <inheritdoc/>
        public Task DeleteAsync(string blobName, string containerName, string userId)
        {
            var path = Path.Combine(FileSystem.AppDataDirectory, "images", userId, "license", blobName);
            _logger.LogInformation("MockBlobStorage: Deleting file {Path}", path);

            if (File.Exists(path))
                File.Delete(path);

            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task<string> GetBlobUrl(string blobName, string containerName, string userId)
        {
            var fullPath = Path.Combine(FileSystem.AppDataDirectory, "images", userId, "license", blobName);
            var localUrl = $"file://{fullPath}";

            _logger.LogInformation("MockBlobStorage: Returning local URL {Url}", localUrl);
            return Task.FromResult(localUrl);
        }

        /// <inheritdoc/>
        public Task<string> GenerateUploadSasUrl(string blobName, string containerName, TimeSpan expiry, string userId)
        {
            var fakeUrl = $"https://mock.truckero.dev/{containerName}/{userId}/license/{blobName}?sas=fake";
            _logger.LogInformation("MockBlobStorage: Generated fake upload SAS URL {Url}", fakeUrl);
            return Task.FromResult(fakeUrl);
        }

        /// <inheritdoc/>
        public Task<string> GenerateDownloadSasUrl(string blobName, string containerName, TimeSpan expiry, string userId)
        {
            var fakeUrl = $"https://mock.truckero.dev/{containerName}/{userId}/license/{blobName}?sas=fake";
            _logger.LogInformation("MockBlobStorage: Generated fake download SAS URL {Url}", fakeUrl);
            return Task.FromResult(fakeUrl);
        }

        public Task<string> UploadAsync(Stream fileStream, string fileName, string containerName, string contentType)
        {
            throw new NotImplementedException();
        }

        public Task<Stream?> DownloadAsync(string blobName, string containerName)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(string blobName, string containerName)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetBlobUrl(string blobName, string containerName)
        {
            throw new NotImplementedException();
        }

        public Task<string> GenerateUploadSasUrl(string blobName, string containerName, TimeSpan expiry)
        {
            throw new NotImplementedException();
        }

        public Task<string> GenerateDownloadSasUrl(string blobName, string containerName, TimeSpan expiry)
        {
            throw new NotImplementedException();
        }
    }
}
