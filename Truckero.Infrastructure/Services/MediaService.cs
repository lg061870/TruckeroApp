using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;
using Truckero.Core.DTOs.Media;
using Truckero.Core.Interfaces;
using Truckero.Core.Interfaces.Services;

namespace Truckero.Infrastructure.Services
{
    /// <summary>
    /// Implementation of the media service for handling image uploads and management
    /// </summary>
    public class MediaService : IMediaService
    {
        private readonly IBlobStorageService _blobStorageService;
        private readonly ILogger<MediaService> _logger;
        private const string ImageContainer = "images";

        public MediaService(IBlobStorageService blobStorageService, ILogger<MediaService> logger)
        {
            _blobStorageService = blobStorageService;
            _logger = logger;
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
                    ImageContainer, 
                    contentType);

                _logger.LogInformation("Image uploaded successfully: {FileName}", uniqueFileName);
                
                return ImageUploadResponse.Succeeded(url, uniqueFileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading image");
                return ImageUploadResponse.Failed($"Error uploading image: {ex.Message}");
            }
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteImageAsync(string fileId)
        {
            try
            {
                await _blobStorageService.DeleteAsync(fileId, ImageContainer);
                _logger.LogInformation("Image deleted successfully: {FileId}", fileId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting image: {FileId}", fileId);
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<string> GetTemporaryUrlAsync(string fileId, int expirationMinutes = 60)
        {
            try
            {
                var expiry = TimeSpan.FromMinutes(expirationMinutes);
                string url = await _blobStorageService.GenerateDownloadSasUrl(fileId, ImageContainer, expiry);
                return url;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating temporary URL for image: {FileId}", fileId);
                throw;
            }
        }
    }
}
