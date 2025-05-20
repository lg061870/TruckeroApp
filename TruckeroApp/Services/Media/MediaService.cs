using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using Truckero.Core.DTOs.Media;
using Truckero.Core.Interfaces;
using Truckero.Core.Interfaces.Services;
using System.IO;

namespace TruckeroApp.Services.Media
{
    /// <summary>
    /// Implementation of the IMediaService for handling media uploads in the Truckero app
    /// </summary>
    public class MediaService : IMediaService
    {
        private readonly ILogger<MediaService> _logger;
        private readonly IBlobStorageService _blobStorageService;

        public MediaService(ILogger<MediaService> logger, IBlobStorageService blobStorageService)
        {
            _logger = logger;
            _blobStorageService = blobStorageService;
        }

        /// <inheritdoc/>
        public async Task<ImageUploadResponse> UploadImageAsync(ImageUploadRequest request)
        {
            try
            {
                if (request.FileStream == null || request.FileStream.Length == 0)
                {
                    return ImageUploadResponse.Failed("File is empty or null");
                }

                // Validate file type
                if (string.IsNullOrEmpty(request.FileType))
                {
                    return ImageUploadResponse.Failed("File type is required");
                }

                // Validate file size
                if (request.FileStream.Length > request.MaxSizeBytes)
                {
                    return ImageUploadResponse.Failed($"File size exceeds the maximum allowed size of {request.MaxSizeBytes / 1024 / 1024}MB");
                }

                // Generate a unique file name
                string fileExtension = Path.GetExtension(request.FileName).ToLowerInvariant();
                
                // Validate file extension
                if (!request.AllowedExtensions.Contains(fileExtension))
                {
                    return ImageUploadResponse.Failed($"File extension {fileExtension} is not allowed. Allowed extensions: {string.Join(", ", request.AllowedExtensions)}");
                }

                // Create a unique file name with original extension
                string uniqueFileName = $"{request.FileType}/{Guid.NewGuid()}{fileExtension}";
                
                // Determine content type based on extension
                string contentType = fileExtension switch
                {
                    ".jpg" or ".jpeg" => "image/jpeg",
                    ".png" => "image/png",
                    ".gif" => "image/gif",
                    _ => "application/octet-stream"
                };

                // Upload to blob storage
                string url = await _blobStorageService.UploadAsync(
                    request.FileStream, 
                    uniqueFileName, 
                    "images", 
                    contentType);

                _logger.LogInformation("Image uploaded successfully: {FileName}", uniqueFileName);
                
                return ImageUploadResponse.Succeeded(url, uniqueFileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading image: {Message}", ex.Message);
                return ImageUploadResponse.Failed($"Error uploading image: {ex.Message}");
            }
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteImageAsync(string fileId)
        {
            try
            {
                // Extract container and blob name from fileId
                string[] parts = fileId.Split('/');
                if (parts.Length < 2)
                {
                    _logger.LogError("Invalid file ID format: {FileId}", fileId);
                    return false;
                }

                string blobName = parts[1]; // The actual filename
                string containerName = "images";

                await _blobStorageService.DeleteAsync(blobName, containerName);
                _logger.LogInformation("Image deleted successfully: {FileId}", fileId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting image: {Message}", ex.Message);
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<string> GetTemporaryUrlAsync(string fileId, int expirationMinutes = 60)
        {
            try
            {
                // Extract container and blob name from fileId
                string[] parts = fileId.Split('/');
                if (parts.Length < 2)
                {
                    _logger.LogError("Invalid file ID format: {FileId}", fileId);
                    throw new ArgumentException("Invalid file ID format", nameof(fileId));
                }

                string blobName = parts[1]; // The actual filename
                string containerName = "images";

                // Generate a SAS URL with the specified expiration
                var expiry = TimeSpan.FromMinutes(expirationMinutes);
                return await _blobStorageService.GenerateDownloadSasUrl(blobName, containerName, expiry);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating temporary URL: {Message}", ex.Message);
                throw;
            }
        }
    }
}
